using System.Security.Claims;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Articles.Manage;

public class DeleteModel : PageModel
{
    private readonly IArticleService _articleService;
    public DeleteModel(IArticleService articleService) => _articleService = articleService;

    public ArticleDetailDto? Article { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Article = await _articleService.GetByIdAsync(id);
        if (Article is null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (Article.AuthorId != userId && !User.IsInRole("Admin"))
            return Forbid();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.DeleteAsync(id, userId, User.IsInRole("Admin"));

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToPage("/Articles/Manage/Index");
        }

        TempData["SuccessMessage"] = "Article deleted.";
        return RedirectToPage("/Articles/Manage/Index");
    }
}
