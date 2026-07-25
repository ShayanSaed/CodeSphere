using CodeSphere.Core.Common;
using CodeSphere.Core.Data;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly CodeSphereDbContext _db;
    public CategoryService(CodeSphereDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories
            .Select(c => new CategoryDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                Description = c.Description,
                ArticleCount = c.Articles.Count
            })
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _db.Categories
            .Where(c => c.CategoryID == id)
            .Select(c => new CategoryDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                Description = c.Description,
                ArticleCount = c.Articles.Count
            }).FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> CreateAsync(CategoryDto dto)
    {
        var exists = await _db.Categories.AnyAsync(c => c.CategoryName == dto.CategoryName);
        if (exists)
            return ServiceResult<int>.Fail("A category with this name already exists.");

        var category = new Category { CategoryName = dto.CategoryName.Trim(), Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return ServiceResult<int>.Ok(category.CategoryID);
    }

    public async Task<ServiceResult> UpdateAsync(int id, CategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return ServiceResult.Fail("Category not found.");

        var duplicate = await _db.Categories.AnyAsync(c => c.CategoryName == dto.CategoryName && c.CategoryID != id);
        if (duplicate) return ServiceResult.Fail("A category with this name already exists.");

        category.CategoryName = dto.CategoryName.Trim();
        category.Description = dto.Description;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Articles).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category is null) return ServiceResult.Fail("Category not found.");

        if (category.Articles.Any())
            return ServiceResult.Fail("Cannot delete a category that still has articles assigned to it.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
