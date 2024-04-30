namespace LiquidCode.Models.Api.MissionsController;

public record MissionsPage(bool HasNextPage, IEnumerable<MissionModel> Missions);