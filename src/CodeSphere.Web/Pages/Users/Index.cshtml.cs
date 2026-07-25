using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using CodeSphere.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    public IndexModel(IUserService userService) => _userService = userService;

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<UserListItemDto> Users { get; set; } = new();

    public PaginationViewModel Pagination => new()
    {
        CurrentPage = Users.PageNumber,
        TotalPages = Users.TotalPages,
        PageName = "/Users/Index",
        RouteValues = string.IsNullOrWhiteSpace(Keyword) ? new() : new() { ["Keyword"] = Keyword }
    };

    public async Task OnGetAsync()
    {
        Users = await _userService.SearchUsersAsync(Keyword, PageNumber, 24);
    }
}
