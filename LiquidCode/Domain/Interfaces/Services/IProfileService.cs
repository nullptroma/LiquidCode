using LiquidCode.Api.Profile.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileOverviewResponse?> GetOverviewAsync(string username, int? requesterId, CancellationToken cancellationToken = default);
    Task<ProfileProblemsResponse?> GetProblemsAsync(string username, int? requesterId, ProfileProblemsQuery query, CancellationToken cancellationToken = default);
    Task<ProfileArticlesResponse?> GetArticlesAsync(string username, int? requesterId, ProfileArticlesQuery query, CancellationToken cancellationToken = default);
    Task<ProfileContestsResponse?> GetContestsAsync(string username, int? requesterId, ProfileContestsQuery query, CancellationToken cancellationToken = default);
}

public record ProfileProblemsQuery(int RecentPage, int RecentPageSize, int AuthoredPage, int AuthoredPageSize);

public record ProfileArticlesQuery(int Page, int PageSize);

public record ProfileContestsQuery(
    int UpcomingPage,
    int UpcomingPageSize,
    int PastPage,
    int PastPageSize,
    int MinePage,
    int MinePageSize);
