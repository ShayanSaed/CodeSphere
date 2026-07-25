using System.Security.Claims;
using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using CodeSphere.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Articles.Manage;

public class IndexModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public IndexModel(IArticleService articleService, ICategoryService categoryService, ITagService tagService)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchColumn { get; set; } = "Title";

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? TagId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "Newest";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<ArticleListItemDto> MyArticles { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();

    public PaginationViewModel Pagination => new()
    {
        CurrentPage = MyArticles.PageNumber,
        TotalPages = MyArticles.TotalPages,
        PageName = "/Articles/Manage/Index",
        RouteValues = BuildRouteValues()
    };

    private Dictionary<string, string> BuildRouteValues()
    {
        var values = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Keyword)) values["Keyword"] = Keyword;
        if (!string.IsNullOrWhiteSpace(SearchColumn)) values["SearchColumn"] = SearchColumn;
        if (CategoryId.HasValue) values["CategoryId"] = CategoryId.Value.ToString();
        if (TagId.HasValue) values["TagId"] = TagId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(Status)) values["Status"] = Status;
        if (!string.IsNullOrWhiteSpace(SortBy)) values["SortBy"] = SortBy;
        return values;
    }

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllAsync();
        Tags = await _tagService.GetAllAsync();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        MyArticles = await _articleService.SearchByAuthorAsync(userId, new ArticleSearchFilterDto
        {
            Keyword = Keyword,
            SearchColumn = SearchColumn,
            CategoryId = CategoryId,
            TagId = TagId,
            Status = Status,
            SortBy = SortBy,
            PageNumber = PageNumber,
            PageSize = 10
        });
    }
}
