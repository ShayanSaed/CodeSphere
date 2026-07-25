using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Categories;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public CreateModel(ICategoryService categoryService) => _categoryService = categoryService;

    [BindProperty]
    public CategoryDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _categoryService.CreateAsync(Input);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return Page();
        }

        TempData["SuccessMessage"] = "Category created.";
        return RedirectToPage("/Categories/Index");
    }
}
