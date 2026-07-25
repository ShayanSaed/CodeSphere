namespace CodeSphere.Core.DTOs;

public class TrendingArticleReportRow
{
    public int ArticleID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public int ReactionCount { get; set; }
    public int EngagementScore { get; set; }
}

public class UserActivityReportRow
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int TotalArticles { get; set; }
    public int TotalComments { get; set; }
    public int TotalReactions { get; set; }
    public int TotalFollowers { get; set; }
}

public class DashboardStatsDto
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int TotalUsers { get; set; }
    public int TotalComments { get; set; }
    public int TotalReactions { get; set; }
    public List<CategoryArticleCountDto> ArticlesByCategory { get; set; } = new();
    public List<DailyArticleCountDto> ArticlesOverTime { get; set; } = new();
    public List<TrendingArticleReportRow> TopArticles { get; set; } = new();
}

public class CategoryArticleCountDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyArticleCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
