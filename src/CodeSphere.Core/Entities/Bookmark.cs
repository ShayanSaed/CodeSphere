using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class Bookmark
{
    [Key]
    public int BookmarkID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public int ArticleID { get; set; }

    public DateTime SavedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserID))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(ArticleID))]
    public Article? Article { get; set; }
}
