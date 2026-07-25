using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class UserService : IUserService
{
    private readonly CodeSphereDbContext _db;
    public UserService(CodeSphereDbContext db) => _db = db;

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return null;

        var articles = await _db.Articles
            .Where(a => a.UserID == userId && a.Status == "Published")
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .Include(a => a.Reactions)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .OrderByDescending(a => a.PublishDate)
            .AsNoTracking()
            .Select(a => new ArticleListItemDto
            {
                ArticleID = a.ArticleID,
                Title = a.Title,
                Author = user.UserName ?? "Unknown",
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

        return new UserProfileDto
        {
            UserId = user.Id,
            Username = user.UserName ?? "Unknown",
            JoinDate = user.JoinDate,
            FullName = user.Profile?.FullName,
            Bio = user.Profile?.Bio,
            Country = user.Profile?.Country,
            WebsiteURL = user.Profile?.WebsiteURL,
            ProfileImageURL = user.Profile?.ProfileImageURL,
            FollowerCount = await _db.Follows.CountAsync(f => f.FollowingUserID == userId),
            FollowingCount = await _db.Follows.CountAsync(f => f.FollowerUserID == userId),
            Articles = articles
        };
    }

    public async Task<ServiceResult> UpsertProfileAsync(int userId, string? fullName, string? bio, string? country, string? websiteUrl, string? profileImageUrl)
    {
        if (!ImageUrlValidator.IsValid(profileImageUrl, out var error))
            return ServiceResult.Fail(error!);

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserID == userId);
        if (profile is null)
        {
            profile = new UserProfile { UserID = userId };
            _db.UserProfiles.Add(profile);
        }

        profile.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        profile.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        profile.Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        profile.WebsiteURL = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl.Trim();
        profile.ProfileImageURL = ImageUrlValidator.Normalize(profileImageUrl);

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<PagedResult<UserListItemDto>> SearchUsersAsync(string? keyword, int pageNumber, int pageSize)
    {
        var query = _db.Users.Include(u => u.Profile).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.UserName!, $"%{kw}%") ||
                (u.Profile != null && u.Profile.FullName != null && EF.Functions.Like(u.Profile.FullName, $"%{kw}%")));
        }

        var totalCount = await query.CountAsync();

        var size = pageSize <= 0 ? 12 : Math.Min(pageSize, 50);
        var number = pageNumber <= 0 ? 1 : pageNumber;

        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((number - 1) * size)
            .Take(size)
            .Select(u => new UserListItemDto
            {
                UserId = u.Id,
                Username = u.UserName ?? "Unknown",
                FullName = u.Profile != null ? u.Profile.FullName : null,
                Country = u.Profile != null ? u.Profile.Country : null,
                ProfileImageURL = u.Profile != null ? u.Profile.ProfileImageURL : null,
                ArticleCount = _db.Articles.Count(a => a.UserID == u.Id && a.Status == "Published"),
                FollowerCount = _db.Follows.Count(f => f.FollowingUserID == u.Id)
            })
            .ToListAsync();

        return new PagedResult<UserListItemDto>
        {
            Items = items,
            PageNumber = number,
            PageSize = size,
            TotalCount = totalCount
        };
    }
}
