using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Common;

/// <summary>
/// Shared query-building logic for filtering, sorting, and paging articles.
/// Used by <c>ArticleService</c> (site-wide search, and search within a
/// single author's own articles) and <c>BookmarkService</c> (search within a
/// user's bookmarks), so all three "advanced search" experiences behave
/// identically instead of drifting apart over time.
/// </summary>
public static class ArticleFilterHelper
{
    public static IQueryable<Article> ApplyFilters(IQueryable<Article> query, ArticleSearchFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(a => a.Status == filter.Status);

        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.CategoryID == filter.CategoryId);

        if (filter.TagId.HasValue)
            query = query.Where(a => a.ArticleTags.Any(at => at.TagID == filter.TagId));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = filter.SearchColumn switch
            {
                "Content" => query.Where(a => EF.Functions.Like(a.Content, $"%{keyword}%")),
                _ => query.Where(a => EF.Functions.Like(a.Title, $"%{keyword}%"))
            };
        }

        return query;
    }

    public static IQueryable<Article> ApplySort(IQueryable<Article> query, string? sortBy) => sortBy switch
    {
        "MostViewed" => query.OrderByDescending(a => a.ViewCount),
        "Trending" => query.OrderByDescending(a => a.ViewCount + a.Comments.Count * 3 + a.Reactions.Count * 2),
        _ => query.OrderByDescending(a => a.PublishDate)
    };

    public static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize, int maxPageSize = 50)
    {
        var size = pageSize <= 0 ? 10 : Math.Min(pageSize, maxPageSize);
        var number = pageNumber <= 0 ? 1 : pageNumber;
        return (number, size);
    }
}
