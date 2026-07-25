using System.ComponentModel.DataAnnotations;

namespace CodeSphere.Core.DTOs;

public class CategoryDto
{
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int ArticleCount { get; set; }
}

public class TagDto
{
    public int TagID { get; set; }

    [Required]
    [MaxLength(50)]
    public string TagName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public int ArticleCount { get; set; }
}
