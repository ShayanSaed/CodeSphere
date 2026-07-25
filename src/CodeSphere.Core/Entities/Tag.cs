using System.ComponentModel.DataAnnotations;

namespace CodeSphere.Core.Entities;

public class Tag
{
    [Key]
    public int TagID { get; set; }

    [Required]
    [MaxLength(50)]
    public string TagName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
