using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    public TagsController(ITagService tagService) => _tagService = tagService;

    /// <summary>GET /api/tags</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<TagDto>>> GetAll() => Ok(await _tagService.GetAllAsync());

    /// <summary>GET /api/tags/5</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<TagDto>> GetById(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        return tag is null ? NotFound() : Ok(tag);
    }

    /// <summary>POST /api/tags (Admin only)</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Create(TagDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _tagService.CreateAsync(dto);
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        var created = await _tagService.GetByIdAsync(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, created);
    }

    /// <summary>DELETE /api/tags/5 (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tagService.DeleteAsync(id);
        return result.Success ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }
}
