using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface IBookmarkService
{
    Task<List<ArticleListItemDto>> GetByUserAsync(int userId);
    Task<ServiceResult<bool>> ToggleAsync(int userId, int articleId);

    /// <summary>Paged, filtered search restricted to one user's bookmarked articles — powers the advanced search on /Bookmarks.</summary>
    Task<PagedResult<ArticleListItemDto>> SearchBookmarksAsync(int userId, ArticleSearchFilterDto filter);
}
