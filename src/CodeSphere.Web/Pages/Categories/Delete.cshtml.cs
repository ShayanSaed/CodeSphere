using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Categories;

[Authorize(Policy = "AdminOnly")]
public class DeleteModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public DeleteModel(ICategoryService categoryService) => _categoryService = categoryService;

    public CategoryDto? Category { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Category = await _categoryService.GetByIdAsync(id);
        if (Category is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Category deleted." : result.ErrorMessage;
        return RedirectToPage("/Categories/Index");
    }
}
