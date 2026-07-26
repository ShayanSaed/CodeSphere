using System.Security.Claims;
using System.Text.Json;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Articles;

public class DetailsModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly IReactionService _reactionService;
    private readonly ICommentService _commentService;
    private readonly IBookmarkService _bookmarkService;
    private readonly IFollowService _followService;

    public DetailsModel(
        IArticleService articleService,
        IReactionService reactionService,
        ICommentService commentService,
        IBookmarkService bookmarkService,
        IFollowService followService)
    {
        _articleService = articleService;
        _reactionService = reactionService;
        _commentService = commentService;
        _bookmarkService = bookmarkService;
        _followService = followService;
    }

    public ArticleDetailDto? Article { get; set; }
    public bool IsBookmarked { get; set; }
    public bool IsFollowingAuthor { get; set; }
    public int AuthorFollowerCount { get; set; }

    private int? CurrentUserId =>
        User.Identity?.IsAuthenticated == true
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : null;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Article = await _articleService.GetByIdAsync(id, trackView: true);
        if (Article is null) return NotFound();

        if (CurrentUserId is int userId)
        {
            var mine = await _bookmarkService.GetByUserAsync(userId);
            IsBookmarked = mine.Any(a => a.ArticleID == id);
            IsFollowingAuthor = await _followService.IsFollowingAsync(userId, Article.AuthorId);
        }
        AuthorFollowerCount = await _followService.GetFollowerCountAsync(Article.AuthorId);

        return Page();
    }

    // ---------------- AJAX handler: POST ?handler=Reaction&articleId=&type= ----------------
    public async Task<IActionResult> OnPostReactionAsync(int articleId, string type)
    {
        if (CurrentUserId is not int userId) return Unauthorized();

        var result = await _reactionService.ToggleAsync(articleId, userId, type);
        // if (!result.Success) return BadRequest(result.ErrorMessage);
        if (!result.Success)
            return BadRequest(result.ErrorMessage ?? "An unknown error occurred.");

        return new JsonResult(result.Data);
    }

    // ---------------- AJAX handler: POST ?handler=Bookmark&articleId= ----------------
    public async Task<IActionResult> OnPostBookmarkAsync(int articleId)
    {
        if (CurrentUserId is not int userId) return Unauthorized();

        var result = await _bookmarkService.ToggleAsync(userId, articleId);
        // if (!result.Success) return BadRequest(result.ErrorMessage);
        if (!result.Success)
            return BadRequest(result.ErrorMessage ?? "An unknown error occurred.");

        return new JsonResult(result.Data);
    }

    // ---------------- AJAX handler: POST ?handler=Comment&articleId=  body: { commentText } ----------------
    public async Task<IActionResult> OnPostCommentAsync(int articleId)
    {
        if (CurrentUserId is not int userId) return Unauthorized();

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var payload = JsonSerializer.Deserialize<CommentPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var result = await _commentService.AddAsync(articleId, userId, payload?.CommentText ?? string.Empty);
        // if (!result.Success) return BadRequest(result.ErrorMessage);
        if (!result.Success)
            return BadRequest(result.ErrorMessage ?? "An unknown error occurred.");

        return new JsonResult(result.Data);
    }

    public async Task<IActionResult> OnPostFollowAsync(int authorId)
    {
        if (CurrentUserId is not int userId) return Unauthorized();

        var result = await _followService.ToggleAsync(userId, authorId);
        // if (!result.Success) return BadRequest(result.ErrorMessage);
        if (!result.Success)
            return BadRequest(result.ErrorMessage ?? "An unknown error occurred.");

        return new JsonResult(result.Data);
    }

    private class CommentPayload
    {
        public string? CommentText { get; set; }
    }
}
