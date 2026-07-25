using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(int id);
    Task<ServiceResult<int>> CreateAsync(TagDto dto);
    Task<ServiceResult> UpdateAsync(int id, TagDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
