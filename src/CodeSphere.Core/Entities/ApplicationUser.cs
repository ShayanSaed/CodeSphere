using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CodeSphere.Core.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's user with the domain fields from the
/// original CodeSphere.Users table (JoinDate, Status). Username/Email/
/// PasswordHash are already provided by IdentityUser&lt;int&gt;.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    [Required]
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    [Required]
    [RegularExpression("Active|Deactive", ErrorMessage = "Status must be Active or Deactive.")]
    public string Status { get; set; } = "Active";

    public UserProfile? Profile { get; set; }
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
}
