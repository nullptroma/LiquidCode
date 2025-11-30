using System.Collections.Generic;

namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Универсальный ответ со страницей данных
/// </summary>
/// <param name="Items">Элементы текущей страницы</param>
/// <param name="Page">Номер страницы (0-based)</param>
/// <param name="PageSize">Количество элементов на странице</param>
/// <param name="HasNextPage">Признак наличия следующей страницы</param>
public record ProfilePagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    bool HasNextPage);
