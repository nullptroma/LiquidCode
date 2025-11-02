using System.ComponentModel.DataAnnotations;
using LiquidCode.Api.Submits.Dto;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность решения с индексацией и временными метками
/// </summary>
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class DbSolution : ITimestamped
{
    public int Id { get; set; }
    
    [Required] 
    public DbMission Mission { get; init; } = null!;
    
    [StringLength(16)] 
    [Required] 
    public string Language { get; init; } = null!;
    
    [StringLength(16)] 
    [Required] 
    public string LanguageVersion { get; init; } = null!;
    
    [StringLength(10000)] 
    [Required] 
    public string SourceCode { get; init; } = null!;
    
    [StringLength(256)] 
    [Required] 
    public string Status { get; set; } = null!;

    public TesterState TestingState { get; set; } = TesterState.Waiting;

    public TesterErrorCode TestingErrorCode { get; set; } = TesterErrorCode.None;

    [StringLength(512)]
    public string? TestingMessage { get; set; }

    public int CurrentTest { get; set; }

    public int AmountOfTests { get; set; }

    public DateTime Time { get; init; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}