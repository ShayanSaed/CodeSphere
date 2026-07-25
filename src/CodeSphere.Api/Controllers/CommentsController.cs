using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/articles/{articleId:int}/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    public CommentsController(ICommentService commentService) => _commentService = commentService;

    /// <summary>GET /api/articles/5/comments</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CommentDto>>> GetByArticle(int articleId) =>
        Ok(await _commentService.GetByArticleAsync(articleId));

    /// <summary>POST /api/articles/5/comments — add a comment as the authenticated user.</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Add(int articleId, [FromBody] CommentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _commentService.AddAsync(articleId, userId, dto.CommentText);
        return result.Success ? Ok(result.Data) : BadRequest(new { message = result.ErrorMessage });
    }

    /// <summary>DELETE /api/articles/5/comments/12 — delete your own comment (or any, if Admin).</summary>
    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int articleId, int commentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _commentService.DeleteAsync(commentId, userId, User.IsInRole("Admin"));
        return result.Success ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }
}
