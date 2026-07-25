namespace CodeSphere.Core.DTOs;

/// <summary>Lightweight summary used by the Users directory page (/Users) — full detail lives in UserProfileDto.</summary>
public class UserListItemDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageURL { get; set; }
    public int ArticleCount { get; set; }
    public int FollowerCount { get; set; }
}
