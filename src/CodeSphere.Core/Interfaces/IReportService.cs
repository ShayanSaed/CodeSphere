using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface IReportService
{
    Task<List<TrendingArticleReportRow>> GetTrendingArticlesAsync(int top = 20);
    Task<List<UserActivityReportRow>> GetUserActivityAsync();
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
