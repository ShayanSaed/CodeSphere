using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class CommentService : ICommentService
{
    private readonly CodeSphereDbContext _db;
    public CommentService(CodeSphereDbContext db) => _db = db;

    public async Task<List<CommentDto>> GetByArticleAsync(int articleId)
    {
        return await _db.Comments
            .Include(c => c.User)
            .Where(c => c.ArticleID == articleId)
            .OrderByDescending(c => c.CommentDate)
            .Select(c => new CommentDto
            {
                CommentID = c.CommentID,
                ArticleID = c.ArticleID,
                Author = c.User != null ? c.User.UserName ?? "Unknown" : "Unknown",
                UserID = c.UserID,
                CommentText = c.CommentText,
                CommentDate = c.CommentDate
            }).ToListAsync();
    }

    public async Task<ServiceResult<CommentDto>> AddAsync(int articleId, int userId, string commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
            return ServiceResult<CommentDto>.Fail("Comment cannot be empty.");

        var articleExists = await _db.Articles.AnyAsync(a => a.ArticleID == articleId);
        if (!articleExists)
            return ServiceResult<CommentDto>.Fail("Article not found.");

        var comment = new Comment { ArticleID = articleId, UserID = userId, CommentText = commentText.Trim() };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return ServiceResult<CommentDto>.Ok(new CommentDto
        {
            CommentID = comment.CommentID,
            ArticleID = articleId,
            Author = user?.UserName ?? "Unknown",
            UserID = userId,
            CommentText = comment.CommentText,
            CommentDate = comment.CommentDate
        });
    }

    public async Task<ServiceResult> DeleteAsync(int commentId, int currentUserId, bool isAdmin)
    {
        var comment = await _db.Comments.FindAsync(commentId);
        if (comment is null)
            return ServiceResult.Fail("Comment not found.");

        if (comment.UserID != currentUserId && !isAdmin)
            return ServiceResult.Fail("You are not allowed to delete this comment.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
