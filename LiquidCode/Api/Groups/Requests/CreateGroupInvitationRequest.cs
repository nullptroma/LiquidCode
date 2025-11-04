using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание приглашения в группу
/// </summary>
public record CreateGroupInvitationRequest(
    [Required]
    string Target,
    GroupInvitationDeliveryChannel DeliveryChannel
);
