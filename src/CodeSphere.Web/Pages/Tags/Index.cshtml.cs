using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using CodeSphere.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Tags;

public class IndexModel : PageModel
{
    private readonly ITagService _tagService;
    public IndexModel(ITagService tagService) => _tagService = tagService;

    private const int PageSize = 60;

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<TagDto> Tags { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PaginationViewModel Pagination => new()
    {
        CurrentPage = PageNumber <= 0 ? 1 : PageNumber,
        TotalPages = TotalPages,
        PageName = "/Tags/Index",
        RouteValues = string.IsNullOrWhiteSpace(Keyword) ? new() : new() { ["Keyword"] = Keyword }
    };

    public async Task OnGetAsync()
    {
        var all = await _tagService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var kw = Keyword.Trim();
            all = all.Where(t => t.TagName.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        TotalCount = all.Count;
        var pageNumber = PageNumber <= 0 ? 1 : PageNumber;

        Tags = all
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
