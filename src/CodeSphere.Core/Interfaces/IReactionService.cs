using CodeSphere.Core.Common;

namespace CodeSphere.Core.Interfaces;

public interface IReactionService
{
    Task<Dictionary<string, int>> GetBreakdownAsync(int articleId);
    Task<ServiceResult<Dictionary<string, int>>> ToggleAsync(int articleId, int userId, string reactionType);
}
