using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

/// <summary>
/// Re-implements, in LINQ, the reporting logic that used to live in the
/// TrendingArticlesView / UserActivityView SQL views and the
/// GetEngagementScore / GetTotalFollowers scalar functions.
/// </summary>
public class ReportService : IReportService
{
    private readonly CodeSphereDbContext _db;
    public ReportService(CodeSphereDbContext db) => _db = db;

    public async Task<List<TrendingArticleReportRow>> GetTrendingArticlesAsync(int top = 50)
    {
        var query = _db.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .Include(a => a.Reactions)
            .Select(a => new TrendingArticleReportRow
            {
                ArticleID = a.ArticleID,
                Title = a.Title,
                Author = a.Author != null ? a.Author.UserName ?? "Unknown" : "Unknown",
                CategoryName = a.Category != null ? a.Category.CategoryName : "Uncategorized",
                ViewCount = a.ViewCount,
                CommentCount = a.Comments.Count,
                ReactionCount = a.Reactions.Count,
                EngagementScore = a.ViewCount + a.Comments.Count * 3 + a.Reactions.Count * 2
            })
            .OrderByDescending(r => r.EngagementScore);

        return top > 0 ? await query.Take(top).ToListAsync() : await query.ToListAsync();
    }

    public async Task<List<UserActivityReportRow>> GetUserActivityAsync(int top = 50)
    {
        var query = _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Articles)
            .Include(u => u.Comments)
            .Include(u => u.Reactions)
            .Select(u => new UserActivityReportRow
            {
                UserID = u.Id,
                Username = u.UserName ?? "Unknown",
                FullName = u.Profile != null ? u.Profile.FullName : null,
                TotalArticles = u.Articles.Count,
                TotalComments = u.Comments.Count,
                TotalReactions = u.Reactions.Count,
                TotalFollowers = _db.Follows.Count(f => f.FollowingUserID == u.Id)
            })
            .OrderByDescending(r => r.TotalArticles);

        return top > 0
            ? await query.Take(top).ToListAsync()
            : await query.ToListAsync();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var stats = new DashboardStatsDto
        {
            TotalArticles = await _db.Articles.CountAsync(),
            PublishedArticles = await _db.Articles.CountAsync(a => a.Status == "Published"),
            DraftArticles = await _db.Articles.CountAsync(a => a.Status == "Draft"),
            TotalUsers = await _db.Users.CountAsync(),
            TotalComments = await _db.Comments.CountAsync(),
            TotalReactions = await _db.Reactions.CountAsync()
        };

        stats.ArticlesByCategory = await _db.Categories
            .Select(c => new CategoryArticleCountDto { CategoryName = c.CategoryName, Count = c.Articles.Count })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        var since = DateTime.UtcNow.AddDays(-13).Date;
        var raw = await _db.Articles
            .Where(a => a.PublishDate != null && a.PublishDate >= since)
            .GroupBy(a => a.PublishDate!.Value.Date)
            .Select(g => new DailyArticleCountDto { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.ArticlesOverTime = Enumerable.Range(0, 14)
            .Select(offset => since.AddDays(offset))
            .Select(d => raw.FirstOrDefault(r => r.Date == d) ?? new DailyArticleCountDto { Date = d, Count = 0 })
            .ToList();

        stats.TopArticles = await GetTrendingArticlesAsync(5);

        return stats;
    }
}
