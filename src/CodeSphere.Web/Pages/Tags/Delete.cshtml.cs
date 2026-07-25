using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Tags;

[Authorize(Policy = "AdminOnly")]
public class DeleteModel : PageModel
{
    private readonly ITagService _tagService;
    public DeleteModel(ITagService tagService) => _tagService = tagService;

    public TagDto? Tag { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tag = await _tagService.GetByIdAsync(id);
        if (Tag is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _tagService.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Tag deleted." : result.ErrorMessage;
        return RedirectToPage("/Tags/Index");
    }
}
