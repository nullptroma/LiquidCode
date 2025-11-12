using LiquidCode.Api.Profile.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDetailsResponse?> GetProfileAsync(string username, int? requesterId, CancellationToken cancellationToken = default);
}
