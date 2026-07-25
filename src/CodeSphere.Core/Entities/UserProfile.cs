using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class UserProfile
{
    [Key]
    public int ProfileID { get; set; }

    [Required]
    public int UserID { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string? WebsiteURL { get; set; }

    [MaxLength(255)]
    public string? ProfileImageURL { get; set; }

    [ForeignKey(nameof(UserID))]
    public ApplicationUser? User { get; set; }
}
