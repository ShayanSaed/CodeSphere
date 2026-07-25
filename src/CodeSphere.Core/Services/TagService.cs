using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class TagService : ITagService
{
    private readonly CodeSphereDbContext _db;
    public TagService(CodeSphereDbContext db) => _db = db;

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await _db.Tags
            .Select(t => new TagDto
            {
                TagID = t.TagID,
                TagName = t.TagName,
                Description = t.Description,
                ArticleCount = t.ArticleTags.Count
            })
            .OrderBy(t => t.TagName)
            .ToListAsync();
    }

    public async Task<TagDto?> GetByIdAsync(int id)
    {
        return await _db.Tags
            .Where(t => t.TagID == id)
            .Select(t => new TagDto
            {
                TagID = t.TagID,
                TagName = t.TagName,
                Description = t.Description,
                ArticleCount = t.ArticleTags.Count
            }).FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> CreateAsync(TagDto dto)
    {
        var exists = await _db.Tags.AnyAsync(t => t.TagName == dto.TagName);
        if (exists) return ServiceResult<int>.Fail("A tag with this name already exists.");

        var tag = new Tag { TagName = dto.TagName.Trim(), Description = dto.Description };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return ServiceResult<int>.Ok(tag.TagID);
    }

    public async Task<ServiceResult> UpdateAsync(int id, TagDto dto)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag is null) return ServiceResult.Fail("Tag not found.");

        var duplicate = await _db.Tags.AnyAsync(t => t.TagName == dto.TagName && t.TagID != id);
        if (duplicate) return ServiceResult.Fail("A tag with this name already exists.");

        tag.TagName = dto.TagName.Trim();
        tag.Description = dto.Description;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag is null) return ServiceResult.Fail("Tag not found.");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
