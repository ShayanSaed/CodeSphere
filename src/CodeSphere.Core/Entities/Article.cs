using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class Article
{
    [Key]
    public int ArticleID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required(ErrorMessage = "Please choose a category.")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content cannot be empty.")]
    public string Content { get; set; } = string.Empty;

    public DateTime? PublishDate { get; set; }

    [Range(0, int.MaxValue)]
    public int ViewCount { get; set; } = 0;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Reading time must be greater than zero.")]
    public int ReadingTime { get; set; } = 1;

    [Required]
    [RegularExpression("Draft|Published", ErrorMessage = "Status must be Draft or Published.")]
    public string Status { get; set; } = "Draft";

    [ForeignKey(nameof(UserID))]
    public ApplicationUser? Author { get; set; }

    [ForeignKey(nameof(CategoryID))]
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    [NotMapped]
    public int EngagementScore => ViewCount + (Comments.Count * 3) + (Reactions.Count * 2);
}
