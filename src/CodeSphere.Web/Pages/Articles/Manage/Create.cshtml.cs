using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Articles.Manage;

public class CreateModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public CreateModel(IArticleService articleService, ICategoryService categoryService, ITagService tagService)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    [BindProperty]
    public ArticleCreateDto Input { get; set; } = new();

    public List<CategoryDto> Categories { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();

    public string TagsJson => System.Text.Json.JsonSerializer.Serialize(
        Tags.Select(t => new { id = t.TagID, name = t.TagName }));

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllAsync();
        Tags = await _tagService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _categoryService.GetAllAsync();
            Tags = await _tagService.GetAllAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.CreateAsync(userId, Input);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create the article.");
            Categories = await _categoryService.GetAllAsync();
            Tags = await _tagService.GetAllAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Article created successfully.";
        return RedirectToPage("/Articles/Details", new { id = result.Data });
    }
}
