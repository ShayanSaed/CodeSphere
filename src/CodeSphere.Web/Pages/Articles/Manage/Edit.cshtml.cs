using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Articles.Manage;

public class EditModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public EditModel(IArticleService articleService, ICategoryService categoryService, ITagService tagService)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    [BindProperty]
    public ArticleUpdateDto Input { get; set; } = new();

    public List<CategoryDto> Categories { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public HashSet<int> SelectedTagIds { get; set; } = new();

    public string TagsJson => System.Text.Json.JsonSerializer.Serialize(
        Tags.Select(t => new { id = t.TagID, name = t.TagName }));

    public string InitialTagsJson => System.Text.Json.JsonSerializer.Serialize(
        Tags.Where(t => SelectedTagIds.Contains(t.TagID)).Select(t => new { id = t.TagID, name = t.TagName }));

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var article = await _articleService.GetByIdAsync(id);
        if (article is null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (article.AuthorId != userId && !User.IsInRole("Admin"))
            return Forbid();

        Categories = await _categoryService.GetAllAsync();
        Tags = await _tagService.GetAllAsync();
        var allTags = Tags;
        SelectedTagIds = allTags.Where(t => article.Tags.Contains(t.TagName)).Select(t => t.TagID).ToHashSet();

        Input = new ArticleUpdateDto
        {
            ArticleID = article.ArticleID,
            CategoryID = article.CategoryId,
            Title = article.Title,
            Content = article.Content,
            ReadingTime = article.ReadingTime,
            Status = article.Status,
            TagIds = SelectedTagIds.ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Input.ArticleID = id;

        if (!ModelState.IsValid)
        {
            Categories = await _categoryService.GetAllAsync();
            Tags = await _tagService.GetAllAsync();
            SelectedTagIds = Input.TagIds.ToHashSet();
            return Page();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.UpdateAsync(id, userId, User.IsInRole("Admin"), Input);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update the article.");
            Categories = await _categoryService.GetAllAsync();
            Tags = await _tagService.GetAllAsync();
            SelectedTagIds = Input.TagIds.ToHashSet();
            return Page();
        }

        TempData["SuccessMessage"] = "Article updated successfully.";
        return RedirectToPage("/Articles/Details", new { id });
    }
}
