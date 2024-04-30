using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Models.Database;

[Index(nameof(Username))]
public class DbUser
{
    public int Id { get; init; }
    [StringLength(32, MinimumLength = 4)] public string Username { get; init; } = "";
    [StringLength(256, MinimumLength = 4)] public string Email { get; init; } = "";
    [StringLength(256)] public string PassHash { get; init; } = "";
    [StringLength(512)] public string Salt { get; init; } = "";
}