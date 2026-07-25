using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class FollowService : IFollowService
{
    private readonly CodeSphereDbContext _db;
    public FollowService(CodeSphereDbContext db) => _db = db;

    public Task<int> GetFollowerCountAsync(int userId) =>
        _db.Follows.CountAsync(f => f.FollowingUserID == userId);

    public Task<int> GetFollowingCountAsync(int userId) =>
        _db.Follows.CountAsync(f => f.FollowerUserID == userId);

    public Task<bool> IsFollowingAsync(int followerUserId, int followingUserId) =>
        _db.Follows.AnyAsync(f => f.FollowerUserID == followerUserId && f.FollowingUserID == followingUserId);

    public async Task<ServiceResult<bool>> ToggleAsync(int followerUserId, int followingUserId)
    {
        if (followerUserId == followingUserId)
            return ServiceResult<bool>.Fail("You cannot follow yourself.");

        var targetExists = await _db.Users.AnyAsync(u => u.Id == followingUserId);
        if (!targetExists) return ServiceResult<bool>.Fail("User not found.");

        var existing = await _db.Follows.FirstOrDefaultAsync(f => f.FollowerUserID == followerUserId && f.FollowingUserID == followingUserId);
        if (existing != null)
        {
            _db.Follows.Remove(existing);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(false);
        }

        _db.Follows.Add(new Follow { FollowerUserID = followerUserId, FollowingUserID = followingUserId });
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
