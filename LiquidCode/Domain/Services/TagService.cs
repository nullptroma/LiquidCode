using System;
using System.Linq;
using LiquidCode.Api.Tags.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Tags;

/// <summary>
/// Реализация сервиса тегов
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(ITagRepository tagRepository, ILogger<TagService> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<TagResponse?> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim();
        var existing = await _tagRepository.FindByNamesAsync(new[] { normalized }, cancellationToken);
        if (existing.Count > 0)
            return TagResponse.FromEntity(existing[0]);

        var tag = new DbTag
        {
            Name = normalized,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tagRepository.CreateAsync(tag, cancellationToken);
        _logger.LogInformation("Created tag {TagName} ({TagId})", tag.Name, tag.Id);
        return TagResponse.FromEntity(tag);
    }

    public async Task<IReadOnlyList<TagResponse>> SearchAsync(string? query, int limit, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        var tags = await _tagRepository.SearchAsync(query, limit, cancellationToken);
        return tags.Select(TagResponse.FromEntity).ToList();
    }

    public async Task<bool> DeleteAsync(int tagId, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.FindByIdAsync(tagId, cancellationToken);
        if (tag == null)
            return false;

        await _tagRepository.SoftDeleteAsync(tag, cancellationToken);
        return true;
    }
}
