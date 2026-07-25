using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface ICommentService
{
    Task<List<CommentDto>> GetByArticleAsync(int articleId);
    Task<ServiceResult<CommentDto>> AddAsync(int articleId, int userId, string commentText);
    Task<ServiceResult> DeleteAsync(int commentId, int currentUserId, bool isAdmin);
}
