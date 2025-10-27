using System.Collections.Generic;
using LiquidCode.Api.Articles.Requests;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Articles;

/// <summary>
/// Контроллер для управления статьями
/// </summary>
[Route("articles")]
[ApiController]
public class ArticlesController(IArticleService articleService) : ControllerBase
{
    /// <summary>
    /// Создает новую статью и загружает архив контента
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await articleService.CreateAsync(request, userId, cancellationToken);
        if (result == null)
            return BadRequest("Failed to create article.");

        return Ok(result);
    }

    /// <summary>
    /// Обновляет существующую статью
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromForm] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await articleService.UpdateAsync(id, request, userId, cancellationToken);
        if (result == null)
            return NotFound("Article not found or access denied.");

        return Ok(result);
    }

    /// <summary>
    /// Удаляет статью текущего пользователя
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await articleService.DeleteAsync(id, userId, cancellationToken);
        if (!success)
            return NotFound("Article not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Возвращает список статей с пагинацией и фильтрацией по тегам
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageSize = 10,
        [FromQuery] int page = 0,
        [FromQuery] List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var result = await articleService.GetPageAsync(pageSize, page, tags, cancellationToken);
        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }

    /// <summary>
    /// Возвращает подробную информацию о статье
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await articleService.GetAsync(id, cancellationToken);
        if (result == null)
            return NotFound("Article not found.");

        return Ok(result);
    }
}
