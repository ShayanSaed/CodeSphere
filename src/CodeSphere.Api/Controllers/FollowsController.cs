using System.Security.Claims;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;
    public FollowsController(IFollowService followService) => _followService = followService;

    /// <summary>GET /api/follows/5/counts — follower/following counts for user 5.</summary>
    [HttpGet("{userId:int}/counts")]
    [AllowAnonymous]
    public async Task<ActionResult> GetCounts(int userId) => Ok(new
    {
        followers = await _followService.GetFollowerCountAsync(userId),
        following = await _followService.GetFollowingCountAsync(userId)
    });

    /// <summary>POST /api/follows/5 — toggle following user 5 as the authenticated user.</summary>
    [HttpPost("{userId:int}")]
    [Authorize]
    public async Task<ActionResult<bool>> Toggle(int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _followService.ToggleAsync(currentUserId, userId);
        return result.Success ? Ok(result.Data) : BadRequest(new { message = result.ErrorMessage });
    }
}
