using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;
    public BookmarksController(IBookmarkService bookmarkService) => _bookmarkService = bookmarkService;

    /// <summary>GET /api/bookmarks — the authenticated user's saved articles.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ArticleListItemDto>>> GetMine()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _bookmarkService.GetByUserAsync(userId));
    }

    /// <summary>POST /api/bookmarks/5 — toggle a bookmark on/off for article 5.</summary>
    [HttpPost("{articleId:int}")]
    public async Task<ActionResult<bool>> Toggle(int articleId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookmarkService.ToggleAsync(userId, articleId);
        return result.Success ? Ok(result.Data) : BadRequest(new { message = result.ErrorMessage });
    }
}
