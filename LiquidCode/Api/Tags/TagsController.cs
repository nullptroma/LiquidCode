using LiquidCode.Api.Tags.Requests;
using LiquidCode.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Tags;

/// <summary>
/// Контроллер для управления тегами
/// </summary>
[Route("tags")]
[ApiController]
public class TagsController(ITagService tagService) : ControllerBase
{
    /// <summary>
    /// Создает новый тег
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await tagService.CreateAsync(request.Name, cancellationToken);
        if (result == null)
            return BadRequest("Unable to create tag.");

        return Ok(result);
    }

    /// <summary>
    /// Выполняет поиск тегов по части названия
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var result = await tagService.SearchAsync(query, limit, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Удаляет тег
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var success = await tagService.DeleteAsync(id, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
