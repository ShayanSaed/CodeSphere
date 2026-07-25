using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class Reaction
{
    [Key]
    public int ReactionID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public int ArticleID { get; set; }

    [Required]
    [RegularExpression("Like|Love|Idea", ErrorMessage = "Reaction type must be Like, Love or Idea.")]
    public string ReactionType { get; set; } = "Like";

    public DateTime ReactionDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserID))]
    public ApplicationUser? User { get; set; }

    [ForeignKey(nameof(ArticleID))]
    public Article? Article { get; set; }
}
