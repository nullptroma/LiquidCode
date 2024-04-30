using LiquidCode.Models.Database;

namespace LiquidCode.Models.Api.MissionsController;

public record MissionModel(int Id, int AuthorId, string Name, int Difficulty, DateTime CreatedAt, DateTime UpdatedAt)
{
    public MissionModel(DbMission dbModel) : this(dbModel.Id, dbModel.Author.Id, dbModel.Name, dbModel.Difficulty, dbModel.CreatedAt, dbModel.UpdatedAt)
    {
        
    }
}