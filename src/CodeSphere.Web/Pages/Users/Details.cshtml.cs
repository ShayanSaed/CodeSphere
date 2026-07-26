using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Users;

public class DetailsModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IFollowService _followService;

    public DetailsModel(IUserService userService, IFollowService followService)
    {
        _userService = userService;
        _followService = followService;
    }

    public UserProfileDto? Profile { get; set; }
    public bool IsOwnProfile { get; set; }
    public bool IsFollowing { get; set; }

    private int? CurrentUserId =>
        User.Identity?.IsAuthenticated == true
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : null;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Profile = await _userService.GetProfileAsync(id);
        if (Profile is null) return NotFound();

        if (CurrentUserId is int userId)
        {
            IsOwnProfile = userId == id;
            if (!IsOwnProfile)
                IsFollowing = await _followService.IsFollowingAsync(userId, id);
        }

        return Page();
    }

    // AJAX handler: POST ?handler=Follow — toggle following this profile's user.
    public async Task<IActionResult> OnPostFollowAsync(int id)
    {
        if (CurrentUserId is not int userId) return Unauthorized();

        var result = await _followService.ToggleAsync(userId, id);
        // if (!result.Success) return BadRequest(result.ErrorMessage);
        if (!result.Success)
            return BadRequest(result.ErrorMessage ?? "An unknown error occurred.");

        return new JsonResult(result.Data);
    }
}
