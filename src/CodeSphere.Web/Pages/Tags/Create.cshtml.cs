using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Tags;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ITagService _tagService;
    public CreateModel(ITagService tagService) => _tagService = tagService;

    [BindProperty]
    public TagDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await _tagService.CreateAsync(Input);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return Page();
        }
        TempData["SuccessMessage"] = "Tag created.";
        return RedirectToPage("/Tags/Index");
    }
}
