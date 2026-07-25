using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class ArticleService : IArticleService
{
    private readonly CodeSphereDbContext _db;

    public ArticleService(CodeSphereDbContext db)
    {
        _db = db;
    }

    private IQueryable<Article> BaseQuery() =>
        _db.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .Include(a => a.Reactions)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .AsNoTracking()
            .AsQueryable();

    public async Task<PagedResult<ArticleListItemDto>> SearchAsync(ArticleSearchFilterDto filter) =>
        await RunSearchAsync(BaseQuery(), filter);

    public async Task<PagedResult<ArticleListItemDto>> SearchByAuthorAsync(int authorUserId, ArticleSearchFilterDto filter) =>
        await RunSearchAsync(BaseQuery().Where(a => a.UserID == authorUserId), filter);

    private static async Task<PagedResult<ArticleListItemDto>> RunSearchAsync(IQueryable<Article> query, ArticleSearchFilterDto filter)
    {
        query = ArticleFilterHelper.ApplyFilters(query, filter);

        var totalCount = await query.CountAsync();

        query = ArticleFilterHelper.ApplySort(query, filter.SortBy);
        var (pageNumber, pageSize) = ArticleFilterHelper.NormalizePaging(filter.PageNumber, filter.PageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToListItem(a))
            .ToListAsync();

        return new PagedResult<ArticleListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ArticleDetailDto?> GetByIdAsync(int articleId, bool trackView = false)
    {
        var article = await _db.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .Include(a => a.Comments).ThenInclude(c => c.User)
            .Include(a => a.Reactions)
            .FirstOrDefaultAsync(a => a.ArticleID == articleId);

        if (article is null) return null;

        if (trackView)
        {
            article.ViewCount += 1;
            await _db.SaveChangesAsync();
        }

        return new ArticleDetailDto
        {
            ArticleID = article.ArticleID,
            Title = article.Title,
            Author = article.Author?.UserName ?? "Unknown",
            AuthorId = article.UserID,
            CategoryName = article.Category?.CategoryName ?? "Uncategorized",
            CategoryId = article.CategoryID,
            PublishDate = article.PublishDate,
            ViewCount = article.ViewCount,
            ReadingTime = article.ReadingTime,
            Status = article.Status,
            CommentCount = article.Comments.Count,
            ReactionCount = article.Reactions.Count,
            EngagementScore = article.EngagementScore,
            Content = article.Content,
            Tags = article.ArticleTags.Select(at => at.Tag!.TagName).ToList(),
            Comments = article.Comments
                .OrderByDescending(c => c.CommentDate)
                .Select(c => new CommentDto
                {
                    CommentID = c.CommentID,
                    ArticleID = c.ArticleID,
                    Author = c.User?.UserName ?? "Unknown",
                    UserID = c.UserID,
                    CommentText = c.CommentText,
                    CommentDate = c.CommentDate
                }).ToList(),
            ReactionBreakdown = article.Reactions
                .GroupBy(r => r.ReactionType)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(int authorUserId, ArticleCreateDto dto)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
        if (!categoryExists)
            return ServiceResult<int>.Fail("The specified category does not exist.");

        var article = new Article
        {
            UserID = authorUserId,
            CategoryID = dto.CategoryID,
            Title = dto.Title.Trim(),
            Content = dto.Content,
            ReadingTime = dto.ReadingTime,
            Status = dto.Status,
            ViewCount = 0,
            PublishDate = dto.Status == "Published" ? DateTime.UtcNow : null
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        if (dto.TagIds.Any())
        {
            var validTagIds = await _db.Tags.Where(t => dto.TagIds.Contains(t.TagID)).Select(t => t.TagID).ToListAsync();
            _db.ArticleTags.AddRange(validTagIds.Select(tagId => new ArticleTag { ArticleID = article.ArticleID, TagID = tagId }));
            await _db.SaveChangesAsync();
        }

        return ServiceResult<int>.Ok(article.ArticleID);
    }

    public async Task<ServiceResult> UpdateAsync(int articleId, int currentUserId, bool isAdmin, ArticleUpdateDto dto)
    {
        var article = await _db.Articles.Include(a => a.ArticleTags).FirstOrDefaultAsync(a => a.ArticleID == articleId);
        if (article is null)
            return ServiceResult.Fail("Article not found.");

        if (article.UserID != currentUserId && !isAdmin)
            return ServiceResult.Fail("You are not allowed to edit this article.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
        if (!categoryExists)
            return ServiceResult.Fail("The specified category does not exist.");

        var wasPublished = article.Status == "Published";
        article.Title = dto.Title.Trim();
        article.Content = dto.Content;
        article.CategoryID = dto.CategoryID;
        article.ReadingTime = dto.ReadingTime;
        article.Status = dto.Status;
        if (!wasPublished && dto.Status == "Published")
            article.PublishDate = DateTime.UtcNow;

        _db.ArticleTags.RemoveRange(article.ArticleTags);
        if (dto.TagIds.Any())
        {
            var validTagIds = await _db.Tags.Where(t => dto.TagIds.Contains(t.TagID)).Select(t => t.TagID).ToListAsync();
            _db.ArticleTags.AddRange(validTagIds.Select(tagId => new ArticleTag { ArticleID = article.ArticleID, TagID = tagId }));
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int articleId, int currentUserId, bool isAdmin)
    {
        var article = await _db.Articles.FindAsync(articleId);
        if (article is null)
            return ServiceResult.Fail("Article not found.");

        if (article.UserID != currentUserId && !isAdmin)
            return ServiceResult.Fail("You are not allowed to delete this article.");

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<List<ArticleListItemDto>> GetByAuthorAsync(int userId)
    {
        return await BaseQuery()
            .Where(a => a.UserID == userId)
            .OrderByDescending(a => a.PublishDate)
            .Select(a => MapToListItem(a))
            .ToListAsync();
    }

    private static ArticleListItemDto MapToListItem(Article a) => new()
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
    };
}
