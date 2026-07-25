using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    /// <summary>GET /api/categories</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetAll() => Ok(await _categoryService.GetAllAsync());

    /// <summary>GET /api/categories/5</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>POST /api/categories (Admin only)</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Create(CategoryDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _categoryService.CreateAsync(dto);
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        var created = await _categoryService.GetByIdAsync(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, created);
    }

    /// <summary>PUT /api/categories/5 (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _categoryService.UpdateAsync(id, dto);
        return result.Success ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }

    /// <summary>DELETE /api/categories/5 (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return result.Success ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }
}
