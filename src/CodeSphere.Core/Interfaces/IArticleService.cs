using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface IArticleService
{
    Task<PagedResult<ArticleListItemDto>> SearchAsync(ArticleSearchFilterDto filter);
    Task<ArticleDetailDto?> GetByIdAsync(int articleId, bool trackView = false);
    Task<ServiceResult<int>> CreateAsync(int authorUserId, ArticleCreateDto dto);
    Task<ServiceResult> UpdateAsync(int articleId, int currentUserId, bool isAdmin, ArticleUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int articleId, int currentUserId, bool isAdmin);
    Task<List<ArticleListItemDto>> GetByAuthorAsync(int userId);

    /// <summary>Paged, filtered search restricted to one author's own articles (drafts included) — powers the advanced search on /Articles/Manage.</summary>
    Task<PagedResult<ArticleListItemDto>> SearchByAuthorAsync(int authorUserId, ArticleSearchFilterDto filter);
}
