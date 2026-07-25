using CodeSphere.Core.Common;

namespace CodeSphere.Core.Interfaces;

public interface IFollowService
{
    Task<int> GetFollowerCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);
    Task<bool> IsFollowingAsync(int followerUserId, int followingUserId);
    Task<ServiceResult<bool>> ToggleAsync(int followerUserId, int followingUserId);
}
