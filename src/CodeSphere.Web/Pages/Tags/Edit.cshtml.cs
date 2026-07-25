using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Tags;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ITagService _tagService;
    public EditModel(ITagService tagService) => _tagService = tagService;

    [BindProperty]
    public TagDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag is null) return NotFound();
        Input = tag;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) return Page();
        var result = await _tagService.UpdateAsync(id, Input);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return Page();
        }
        TempData["SuccessMessage"] = "Tag updated.";
        return RedirectToPage("/Tags/Index");
    }
}
