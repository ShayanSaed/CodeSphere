using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class BookmarkService : IBookmarkService
{
    private readonly CodeSphereDbContext _db;
    public BookmarkService(CodeSphereDbContext db) => _db = db;

    public async Task<List<ArticleListItemDto>> GetByUserAsync(int userId)
    {
        return await _db.Bookmarks
            .Where(b => b.UserID == userId)
            .Include(b => b.Article!).ThenInclude(a => a.Author)
            .Include(b => b.Article!).ThenInclude(a => a.Category)
            .Include(b => b.Article!).ThenInclude(a => a.Comments)
            .Include(b => b.Article!).ThenInclude(a => a.Reactions)
            .Include(b => b.Article!).ThenInclude(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .OrderByDescending(b => b.SavedDate)
            .Select(b => new ArticleListItemDto
            {
                ArticleID = b.Article!.ArticleID,
                Title = b.Article.Title,
                Author = b.Article.Author != null ? b.Article.Author.UserName ?? "Unknown" : "Unknown",
                AuthorId = b.Article.UserID,
                CategoryName = b.Article.Category != null ? b.Article.Category.CategoryName : "Uncategorized",
                PublishDate = b.Article.PublishDate,
                ViewCount = b.Article.ViewCount,
                ReadingTime = b.Article.ReadingTime,
                Status = b.Article.Status,
                CommentCount = b.Article.Comments.Count,
                ReactionCount = b.Article.Reactions.Count,
                EngagementScore = b.Article.ViewCount + b.Article.Comments.Count * 3 + b.Article.Reactions.Count * 2,
                Tags = b.Article.ArticleTags.Select(at => at.Tag!.TagName).ToList()
            }).ToListAsync();
    }

    // Powers the advanced search box on /Bookmarks — same filter/sort/paging
    // semantics as the home page search (via ArticleFilterHelper), scoped to
    // only the articles this user has bookmarked.
    public async Task<PagedResult<ArticleListItemDto>> SearchBookmarksAsync(int userId, ArticleSearchFilterDto filter)
    {
        var query = _db.Articles
            .Where(a => a.Bookmarks.Any(b => b.UserID == userId))
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .Include(a => a.Reactions)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .AsNoTracking()
            .AsQueryable();

        query = ArticleFilterHelper.ApplyFilters(query, filter);

        var totalCount = await query.CountAsync();

        query = ArticleFilterHelper.ApplySort(query, filter.SortBy);
        var (pageNumber, pageSize) = ArticleFilterHelper.NormalizePaging(filter.PageNumber, filter.PageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleListItemDto
            {
                ArticleID = a.ArticleID,
                Title = a.Title,
                Author = a.Author != null ? a.Author.UserName ?? "Unknown" : "Unknown",
                AuthorId = a.UserID,
                CategoryName = a.Category != null ? a.Category.CategoryName : "Uncategorized",
                PublishDate = a.PublishDate,
                ViewCount = a.ViewCount,
                ReadingTime = a.ReadingTime,
                Status = a.Status,
                CommentCount = a.Comments.Count,
                ReactionCount = a.Reactions.Count,
                EngagementScore = a.ViewCount + a.Comments.Count * 3 + a.Reactions.Count * 2,
                Tags = a.ArticleTags.Select(at => at.Tag!.TagName).ToList()
            })
            .ToListAsync();

        return new PagedResult<ArticleListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceResult<bool>> ToggleAsync(int userId, int articleId)
    {
        var articleExists = await _db.Articles.AnyAsync(a => a.ArticleID == articleId);
        if (!articleExists) return ServiceResult<bool>.Fail("Article not found.");

        var existing = await _db.Bookmarks.FirstOrDefaultAsync(b => b.UserID == userId && b.ArticleID == articleId);
        if (existing != null)
        {
            _db.Bookmarks.Remove(existing);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(false); // now unbookmarked
        }

        _db.Bookmarks.Add(new Bookmark { UserID = userId, ArticleID = articleId });
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true); // now bookmarked
    }
}
