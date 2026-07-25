namespace CodeSphere.Core.DTOs;

/// <summary>
/// Everything shown on a user's public profile page: the UserProfiles table
/// content in full, plus basic account info and the user's published
/// articles.
/// </summary>
public class UserProfileDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }

    // UserProfiles table content
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Country { get; set; }
    public string? WebsiteURL { get; set; }
    public string? ProfileImageURL { get; set; }

    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }

    public List<ArticleListItemDto> Articles { get; set; } = new();
}
