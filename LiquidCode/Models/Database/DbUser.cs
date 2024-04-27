using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Models.Database;

[Index(nameof(Username))]
public class DbUser
{
    public int Id { get; private set; } = 0;
    [StringLength(32, MinimumLength = 4)] public string Username { get; set; } = "";
    [StringLength(256, MinimumLength = 4)] public string Email { get; set; } = "";
    [StringLength(256)] public string PassHash { get; set; } = "";
    [StringLength(512)] public string Salt { get; set; } = "";
}