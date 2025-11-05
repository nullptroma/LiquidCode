using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Articles.Requests;
using LiquidCode.Api.Articles.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Articles;

/// <summary>
/// Реализация бизнес-логики для статей
/// </summary>
public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(
        IArticleRepository articleRepository,
        IUserRepository userRepository,
        ITagRepository tagRepository,
        ILogger<ArticleService> logger)
    {
        _articleRepository = articleRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<ArticleResponse?> CreateAsync(CreateArticleRequest request, int authorId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            _logger.LogWarning("Article content is empty");
            return null;
        }

        var user = await _userRepository.FindByIdAsync(authorId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Author not found: {AuthorId}", authorId);
            return null;
        }

        var now = DateTime.UtcNow;
        var article = new DbArticle
        {
            AuthorId = user.Id,
            Author = user,
            Name = string.IsNullOrWhiteSpace(request.Name) ? "Untitled article" : request.Name.Trim(),
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _articleRepository.CreateAsync(article, cancellationToken);
        await SyncTagsAsync(article, request.Tags, cancellationToken);

        var full = await _articleRepository.FindWithDetailsAsync(article.Id, cancellationToken);
        return ArticleResponse.FromEntity(full ?? article);
    }

    public async Task<ArticleResponse?> UpdateAsync(int articleId, UpdateArticleRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.FindWithDetailsAsync(articleId, cancellationToken);
        if (article == null)
        {
            _logger.LogWarning("Article not found: {ArticleId}", articleId);
            return null;
        }

        if (article.Author.Id != userId)
        {
            _logger.LogWarning("User {UserId} is not allowed to edit article {ArticleId}", userId, articleId);
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            article.Name = request.Name.Trim();
        }

        if (request.Content != null)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                _logger.LogWarning("Updated content is empty for article {ArticleId}", articleId);
                return null;
            }

            article.Content = request.Content;
        }

        article.UpdatedAt = DateTime.UtcNow;
        await _articleRepository.UpdateAsync(article, cancellationToken);
        await SyncTagsAsync(article, request.Tags, cancellationToken);

        var fresh = await _articleRepository.FindWithDetailsAsync(article.Id, cancellationToken);
        return ArticleResponse.FromEntity(fresh ?? article);
    }

    public async Task<bool> DeleteAsync(int articleId, int userId, CancellationToken cancellationToken = default)
    {
    var article = await _articleRepository.FindWithDetailsAsync(articleId, cancellationToken);
        if (article == null)
            return false;

        if (article.Author.Id != userId)
            return false;

        await _articleRepository.SoftDeleteAsync(article, cancellationToken);
        return true;
    }

    public async Task<ArticleResponse?> GetAsync(int articleId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.FindWithDetailsAsync(articleId, cancellationToken);
        return article == null ? null : ArticleResponse.FromEntity(article);
    }

    public async Task<ArticlesPageResponse?> GetPageAsync(int pageSize, int pageNumber, IEnumerable<string>? tags, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        IEnumerable<int>? tagIds = null;
        if (tags != null)
        {
            var normalized = tags
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count > 0)
            {
                var existing = await _tagRepository.FindByNamesAsync(normalized, cancellationToken);
                tagIds = existing.Select(t => t.Id).ToList();
            }
        }

        var (articles, hasNext) = await _articleRepository.GetFilteredPageAsync(pageSize, pageNumber, tagIds, cancellationToken);
        return new ArticlesPageResponse(hasNext, articles.Select(ArticleResponse.FromEntity));
    }

    public async Task<IReadOnlyList<ArticleResponse>> GetMyArticlesAsync(int authorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var articles = await _articleRepository.GetByAuthorAsync(authorId, cancellationToken);
            return articles.Select(ArticleResponse.FromEntity).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting articles for author {AuthorId}", authorId);
            return Array.Empty<ArticleResponse>();
        }
    }

    private async Task SyncTagsAsync(DbArticle article, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        var normalized = tags?
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (normalized.Count == 0)
        {
            await _articleRepository.SyncTagsAsync(article, Array.Empty<int>(), cancellationToken);
            return;
        }

        var existing = await _tagRepository.FindByNamesAsync(normalized, cancellationToken);
        var allTags = existing.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

        foreach (var tagName in normalized)
        {
            if (allTags.ContainsKey(tagName))
                continue;

            var newTag = new DbTag
            {
                Name = tagName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tagRepository.CreateAsync(newTag, cancellationToken);
            allTags[tagName] = newTag;
        }

        await _articleRepository.SyncTagsAsync(article, allTags.Values.Select(t => t.Id), cancellationToken);
    }
}
