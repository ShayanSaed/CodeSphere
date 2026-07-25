using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class ReactionService : IReactionService
{
    private readonly CodeSphereDbContext _db;
    public ReactionService(CodeSphereDbContext db) => _db = db;

    public async Task<Dictionary<string, int>> GetBreakdownAsync(int articleId)
    {
        return await _db.Reactions
            .Where(r => r.ArticleID == articleId)
            .GroupBy(r => r.ReactionType)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    // Adds the reaction, or removes it if the same user already reacted with the same type ("toggle").
    public async Task<ServiceResult<Dictionary<string, int>>> ToggleAsync(int articleId, int userId, string reactionType)
    {
        if (!new[] { "Like", "Love", "Idea" }.Contains(reactionType))
            return ServiceResult<Dictionary<string, int>>.Fail("Invalid reaction type.");

        var articleExists = await _db.Articles.AnyAsync(a => a.ArticleID == articleId);
        if (!articleExists)
            return ServiceResult<Dictionary<string, int>>.Fail("Article not found.");

        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.ArticleID == articleId && r.UserID == userId && r.ReactionType == reactionType);
        if (existing != null)
        {
            _db.Reactions.Remove(existing);
        }
        else
        {
            // Users can only have one reaction of each type per article; remove other types first (one reaction per user per article).
            var otherReactions = _db.Reactions.Where(r => r.ArticleID == articleId && r.UserID == userId);
            _db.Reactions.RemoveRange(otherReactions);
            _db.Reactions.Add(new Reaction { ArticleID = articleId, UserID = userId, ReactionType = reactionType });
        }

        await _db.SaveChangesAsync();
        var breakdown = await GetBreakdownAsync(articleId);
        return ServiceResult<Dictionary<string, int>>.Ok(breakdown);
    }
}
