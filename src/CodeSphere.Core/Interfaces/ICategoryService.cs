using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<ServiceResult<int>> CreateAsync(CategoryDto dto);
    Task<ServiceResult> UpdateAsync(int id, CategoryDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
