namespace LiquidCode.Models.Api.MissionsController;

public record MissionModel(int Id, int AuthorId, string Name, int Difficulty, DateTime CreatedAt, DateTime UpdatedAt);