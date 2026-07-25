using System.ComponentModel.DataAnnotations;

namespace CodeSphere.Core.Entities;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
