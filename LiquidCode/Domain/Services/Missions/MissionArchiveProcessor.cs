using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiquidCode.Domain.Enums;
using LiquidCode.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Missions;

/// <summary>
/// Вспомогательный класс для обработки архива миссии и извлечения текстов по языкам
/// </summary>
public class MissionArchiveProcessor
{
    private readonly ILogger<MissionArchiveProcessor> _logger;
    private readonly IMediaService _mediaService;
    
    private static readonly string[] IgnoredDirs = { ".pdf", ".html", "tests", "solutions", "scripts", "files" };

    public MissionArchiveProcessor(ILogger<MissionArchiveProcessor> logger, IMediaService mediaService)
    {
        _logger = logger;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Распаковывает архив и извлекает statements по языкам
    /// </summary>
    public Dictionary<string, MissionStatementData> ExtractStatements(string zipFilePath)
    {
        var statements = new Dictionary<string, MissionStatementData>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using (var zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                // Найти каталоги с statements для каждого языка
                var statementDirs = zipArchive.Entries
                    .Select(e => e.FullName)
                    .Where(name => name.Contains("statements/"))
                    .Select(name => ExtractLanguageAndPath(name))
                    .Where(x => x.Language != null)
                    .Distinct()
                    .GroupBy(x => x.Language)
                    .ToDictionary(g => g.Key!, g => g.ToList());

                foreach (var (language, paths) in statementDirs)
                {
                    _logger.LogInformation("Processing statements for language: {Language}", language);
                    var statementData = new MissionStatementData { Language = language };

                    // Группируем файлы по типам в этом языке
                    var entries = zipArchive.Entries
                        .Where(e => paths.Any(p => p.Path != null && e.FullName.StartsWith(p.Path, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    // Извлекаем текстовые файлы (включая примеры)
                    ExtractTextFiles(entries, statementData);

                    // Извлекаем картинки
                    ExtractImageFiles(entries, zipArchive, statementData);

                    statements[language] = statementData;
                }
            }

            _logger.LogInformation("Successfully extracted statements for {LanguageCount} languages", statements.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting statements from archive");
        }

        return statements;
    }

    private (string? Language, string? Path) ExtractLanguageAndPath(string fullPath)
    {
        var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        // Ищем индекс "statements"
        var stmtIndex = System.Array.FindIndex(parts, p => 
            p.Equals("statements", StringComparison.OrdinalIgnoreCase));

        if (stmtIndex >= 0 && stmtIndex + 1 < parts.Length)
        {
            var language = parts[stmtIndex + 1];
            
            // Игнорируем .pdf и .html каталоги
            if (!IgnoredDirs.Any(dir => language.Equals(dir, StringComparison.OrdinalIgnoreCase)))
            {
                var pathBase = string.Join("/", parts.Take(stmtIndex + 2));
                return (language, pathBase);
            }
        }

        return (null, null);
    }

    private void ExtractTextFiles(List<ZipArchiveEntry> entries, MissionStatementData statement)
    {
        var texts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            // Пропускаем директории
            if (entry.FullName.EndsWith("/"))
                continue;

            // Проверяем, является ли это текстовым файлом или примером
            var mediaType = _mediaService.GetMediaType(entry.Name);
            var isExample = entry.Name.StartsWith("example.", StringComparison.OrdinalIgnoreCase);
            
            if (mediaType == MediaType.Documents || isExample)
            {
                var content = ReadZipEntry(entry);
                if (!string.IsNullOrEmpty(content))
                {
                    texts[entry.Name] = content;
                }
            }
        }

        statement.StatementTexts = texts;
    }

    private void ExtractImageFiles(List<ZipArchiveEntry> entries, ZipArchive zipArchive, MissionStatementData statement)
    {
        var imageFiles = entries
            .Where(e => _mediaService.GetMediaType(e.Name) == MediaType.Images)
            .OrderBy(e => e.Name)
            .ToList();

        foreach (var imageFile in imageFiles)
        {
            statement.ImageFiles.Add(new ImageFileData
            {
                FileName = imageFile.Name,
                ZipEntry = imageFile
            });
        }

        _logger.LogInformation("Found {ImageCount} image files for language {Language}", 
            imageFiles.Count, statement.Language);
    }

    private string ReadZipEntry(ZipArchiveEntry entry)
    {
        try
        {
            using (var reader = new StreamReader(entry.Open()))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading zip entry: {EntryName}", entry.Name);
            return string.Empty;
        }
    }
}

/// <summary>
/// Данные текста миссии для конкретного языка
/// </summary>
public class MissionStatementData
{
    public string Language { get; set; } = "";
    
    /// <summary>
    /// Словарь всех текстовых файлов (problem.tex, input.tex, example.01, example.01.a и т.д.)
    /// </summary>
    public Dictionary<string, string> StatementTexts { get; set; } = new();
    
    public List<ImageFileData> ImageFiles { get; set; } = new();
}

/// <summary>
/// Данные об изображении в архиве
/// </summary>
public class ImageFileData
{
    public string FileName { get; set; } = "";
    public ZipArchiveEntry? ZipEntry { get; set; }
}
