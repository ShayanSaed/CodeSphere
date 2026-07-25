using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class Comment
{
    [Key]
    public int CommentID { get; set; }

    [Required]
    public int ArticleID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required(ErrorMessage = "Comment cannot be empty.")]
    [MaxLength(1000)]
    public string CommentText { get; set; } = string.Empty;

    public DateTime CommentDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ArticleID))]
    public Article? Article { get; set; }

    [ForeignKey(nameof(UserID))]
    public ApplicationUser? User { get; set; }
}
