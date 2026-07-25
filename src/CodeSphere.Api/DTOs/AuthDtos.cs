using System.ComponentModel.DataAnnotations;

namespace CodeSphere.Api.DTOs;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters long.")]
    public string Password { get; set; } = string.Empty;

    // ---- Optional UserProfiles fields, collected at registration ----
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(255)]
    public string? WebsiteURL { get; set; }

    /// <summary>
    /// A direct link to a profile photo (http/https only). Validated by
    /// <see cref="CodeSphere.Core.Common.ImageUrlValidator"/> — the same rule
    /// the Razor Pages Register form enforces — before it is ever persisted.
    /// </summary>
    [MaxLength(255)]
    public string? ProfileImageURL { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
