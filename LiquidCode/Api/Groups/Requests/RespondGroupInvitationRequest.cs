using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на ответ по приглашению в группу
/// </summary>
public record RespondGroupInvitationRequest(
    [Required]
    bool Accept
);
