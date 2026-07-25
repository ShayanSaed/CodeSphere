using System.Security.Claims;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/articles/{articleId:int}/[controller]")]
[Produces("application/json")]
public class ReactionsController : ControllerBase
{
    private readonly IReactionService _reactionService;
    public ReactionsController(IReactionService reactionService) => _reactionService = reactionService;

    /// <summary>GET /api/articles/5/reactions — breakdown by reaction type.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Dictionary<string, int>>> GetBreakdown(int articleId) =>
        Ok(await _reactionService.GetBreakdownAsync(articleId));

    /// <summary>POST /api/articles/5/reactions?type=Like — toggle a reaction for the authenticated user.</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Toggle(int articleId, [FromQuery] string type)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _reactionService.ToggleAsync(articleId, userId, type);
        return result.Success ? Ok(result.Data) : BadRequest(new { message = result.ErrorMessage });
    }
}
