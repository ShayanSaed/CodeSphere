using System.Security.Claims;
using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>GET /api/articles — search, filter (category/tag/status) and paginate published articles.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ArticleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ArticleListItemDto>>> Search([FromQuery] ArticleSearchFilterDto filter)
    {
        var result = await _articleService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>GET /api/articles/5 — full article detail including comments and reaction breakdown.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleDetailDto>> GetById(int id)
    {
        var article = await _articleService.GetByIdAsync(id, trackView: true);
        return article is null ? NotFound() : Ok(article);
    }

    /// <summary>GET /api/articles/mine — the authenticated user's own articles (drafts included).</summary>
    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(List<ArticleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArticleListItemDto>>> GetMine()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var articles = await _articleService.GetByAuthorAsync(userId);
        return Ok(articles);
    }

    /// <summary>POST /api/articles — create a new article as the authenticated user.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(ArticleCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.CreateAsync(userId, dto);
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });

        var created = await _articleService.GetByIdAsync(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, created);
    }

    /// <summary>PUT /api/articles/5 — update an article you own (or any article, if Admin).</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, ArticleUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.UpdateAsync(id, userId, User.IsInRole("Admin"), dto);
        if (!result.Success)
        {
            return result.ErrorMessage!.Contains("not allowed")
                ? Forbid()
                : BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>DELETE /api/articles/5 — delete an article you own (or any article, if Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _articleService.DeleteAsync(id, userId, User.IsInRole("Admin"));
        if (!result.Success)
        {
            if (result.ErrorMessage == "Article not found.") return NotFound();
            return Forbid();
        }

        return NoContent();
    }
}
