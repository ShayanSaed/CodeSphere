using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using CodeSphere.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public IndexModel(ICategoryService categoryService) => _categoryService = categoryService;

    private const int PageSize = 18;

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<CategoryDto> Categories { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PaginationViewModel Pagination => new()
    {
        CurrentPage = PageNumber <= 0 ? 1 : PageNumber,
        TotalPages = TotalPages,
        PageName = "/Categories/Index",
        RouteValues = string.IsNullOrWhiteSpace(Keyword) ? new() : new() { ["Keyword"] = Keyword }
    };

    public async Task OnGetAsync()
    {
        var all = await _categoryService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var kw = Keyword.Trim();
            all = all.Where(c => c.CategoryName.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        TotalCount = all.Count;
        var pageNumber = PageNumber <= 0 ? 1 : PageNumber;

        Categories = all
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
