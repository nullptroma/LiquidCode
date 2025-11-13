using System;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Активная ссылка-приглашение для присоединения к группе
/// </summary>
/// <param name="Token">Токен инвайт-ссылки для присоединения к группе</param>
/// <param name="ExpiresAt">Дата и время истечения срока действия ссылки</param>
public record GroupJoinLinkResponse(string Token, DateTime ExpiresAt);
