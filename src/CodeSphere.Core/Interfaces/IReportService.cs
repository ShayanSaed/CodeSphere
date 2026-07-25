using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface IReportService
{
    Task<List<TrendingArticleReportRow>> GetTrendingArticlesAsync(int top = 50);
    Task<List<UserActivityReportRow>> GetUserActivityAsync(int top = 50);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
