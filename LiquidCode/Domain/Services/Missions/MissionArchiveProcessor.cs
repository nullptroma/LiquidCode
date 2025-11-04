using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
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

    private static readonly HashSet<string> IgnoredDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        "tests",
        "solutions",
        "scripts",
        "files"
    };
    private sealed record StatementDirectory(string Language, StatementFormat Format, string Path);


    private static readonly HashSet<string> HtmlStatementTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html",
        ".htm",
        ".css",
        ".js",
        ".json"
    };

    public MissionArchiveProcessor(ILogger<MissionArchiveProcessor> logger, IMediaService mediaService)
    {
        _logger = logger;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Распаковывает архив и извлекает statements по языкам и форматам
    /// </summary>
    public List<MissionStatementData> ExtractStatements(string zipFilePath)
    {
        var statements = new List<MissionStatementData>();

        try
        {
            using (var zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                // Найти каталоги с statements для каждого языка и формата
                var statementDirs = zipArchive.Entries
                    .Where(entry => entry.FullName.Contains("statements/", StringComparison.OrdinalIgnoreCase))
                    .Select(entry => ExtractLanguageAndPath(entry.FullName))
                    .Where(dir => dir is not null)
                    .Select(dir => dir!)
                    .Distinct()
                    .ToList();

                foreach (var directory in statementDirs)
                {
                    _logger.LogInformation(
                        "Processing statements for language {Language} in format {Format}",
                        directory.Language,
                        directory.Format);

                    var statementData = new MissionStatementData
                    {
                        Language = directory.Language,
                        Format = directory.Format
                    };

                    // Группируем файлы по типам в этом языке
                    var entries = zipArchive.Entries
                        .Where(e => e.FullName.StartsWith(directory.Path, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Извлекаем текстовые файлы (включая примеры)
                    ExtractTextFiles(entries, statementData);

                    // Извлекаем картинки
                    ExtractImageFiles(entries, statementData);

                    statements.Add(statementData);
                }
            }

            _logger.LogInformation(
                "Successfully extracted {Count} statement variations from archive",
                statements.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting statements from archive");
        }

        return statements;
    }

    public MissionExecutionLimits ExtractExecutionLimits(string zipFilePath)
    {
        try
        {
            using var zipArchive = ZipFile.OpenRead(zipFilePath);
            var problemEntry = zipArchive.Entries
                .FirstOrDefault(e => string.Equals(e.Name, "problem.xml", StringComparison.OrdinalIgnoreCase));

            if (problemEntry == null)
            {
                _logger.LogWarning("problem.xml not found in archive {ZipFile}", Path.GetFileName(zipFilePath));
                return MissionExecutionLimits.Empty;
            }

            using var stream = problemEntry.Open();
            var document = XDocument.Load(stream);

            var timeLimitValue = document
                .Descendants("time-limit")
                .Select(x => ParseToNullableInt(x.Value))
                .FirstOrDefault(v => v.HasValue);

            var memoryLimitValue = document
                .Descendants("memory-limit")
                .Select(x => ParseToNullableInt(x.Value))
                .FirstOrDefault(v => v.HasValue);

            return new MissionExecutionLimits(timeLimitValue, memoryLimitValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract execution limits from problem.xml");
            return MissionExecutionLimits.Empty;
        }
    }

    private static int? ParseToNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }

    private StatementDirectory? ExtractLanguageAndPath(string fullPath)
    {
        var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Ищем индекс "statements"
        var stmtIndex = System.Array.FindIndex(parts, p =>
            p.Equals("statements", StringComparison.OrdinalIgnoreCase));

        if (stmtIndex < 0 || stmtIndex + 1 >= parts.Length)
        {
            return null;
        }

        var firstSegment = parts[stmtIndex + 1];

        if (firstSegment.StartsWith(".", StringComparison.Ordinal))
        {
            // Каталоги вида .html/<language>/...
            if (string.Equals(firstSegment, ".pdf", StringComparison.OrdinalIgnoreCase) || stmtIndex + 2 >= parts.Length)
            {
                return null;
            }

            var nestedLanguage = parts[stmtIndex + 2];

            if (ShouldIgnoreSegment(nestedLanguage))
            {
                return null;
            }

            var format = ResolveFormat(firstSegment);
            if (format == null)
            {
                return null;
            }

            var pathBase = string.Join("/", parts.Take(stmtIndex + 3)) + "/";

            return new StatementDirectory(nestedLanguage, format.Value, pathBase);
        }

        if (ShouldIgnoreSegment(firstSegment))
        {
            return null;
        }

        var directPathBase = string.Join("/", parts.Take(stmtIndex + 2)) + "/";
        return new StatementDirectory(firstSegment, StatementFormat.Latex, directPathBase);
    }

    private static bool ShouldIgnoreSegment(string segment) =>
        IgnoredDirs.Contains(segment);

    private static StatementFormat? ResolveFormat(string segment)
    {
        if (segment.Equals(".html", StringComparison.OrdinalIgnoreCase))
        {
            return StatementFormat.Html;
        }

        return null;
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

            var extension = Path.GetExtension(entry.Name);
            var isHtmlText = statement.Format == StatementFormat.Html &&
                             HtmlStatementTextExtensions.Contains(extension);

            if (mediaType == MediaType.Documents || isExample || isHtmlText)
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

    private void ExtractImageFiles(List<ZipArchiveEntry> entries, MissionStatementData statement)
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

        _logger.LogInformation(
            "Found {ImageCount} image files for language {Language} (format {Format})",
            imageFiles.Count,
            statement.Language,
            statement.Format);
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

public sealed record MissionExecutionLimits(int? TimeLimitMilliseconds, int? MemoryLimitBytes)
{
    public static MissionExecutionLimits Empty { get; } = new(null, null);
}

/// <summary>
/// Данные текста миссии для конкретного языка
/// </summary>
public class MissionStatementData
{
    public string Language { get; set; } = "";

    /// <summary>
    /// Формат исходных файлов (Latex, Html и т.д.)
    /// </summary>
    public StatementFormat Format { get; set; } = StatementFormat.Latex;
    
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
