using CodeSphere.Core.Common;
using CodeSphere.Core.DTOs;

namespace CodeSphere.Core.Interfaces;

public interface IUserService
{
    /// <summary>Everything needed to render a user's public profile page, or null if the user doesn't exist.</summary>
    Task<UserProfileDto?> GetProfileAsync(int userId);

    /// <summary>
    /// Creates or updates the UserProfiles row for a user. Used both at
    /// registration (Web and API) and by any future "edit profile" feature.
    /// Validates the profile image URL (see <see cref="ImageUrlValidator"/>)
    /// before persisting anything.
    /// </summary>
    Task<ServiceResult> UpsertProfileAsync(int userId, string? fullName, string? bio, string? country, string? websiteUrl, string? profileImageUrl);

    /// <summary>Paged, searchable directory of all users — powers the Users listing page (/Users).</summary>
    Task<PagedResult<UserListItemDto>> SearchUsersAsync(string? keyword, int pageNumber, int pageSize);
}
