using System.ComponentModel.DataAnnotations;

namespace CodeSphere.Core.DTOs;

public class ArticleListItemDto
{
    public int ArticleID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }
    public int ViewCount { get; set; }
    public int ReadingTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CommentCount { get; set; }
    public int ReactionCount { get; set; }
    public int EngagementScore { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class ArticleDetailDto : ArticleListItemDto
{
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public Dictionary<string, int> ReactionBreakdown { get; set; } = new();
}

public class ArticleCreateDto
{
    [Required(ErrorMessage = "Please choose a category.")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content cannot be empty.")]
    [MinLength(20, ErrorMessage = "Content should be at least 20 characters.")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 240, ErrorMessage = "Reading time must be between 1 and 240 minutes.")]
    public int ReadingTime { get; set; } = 5;

    [Required]
    [RegularExpression("Draft|Published")]
    public string Status { get; set; } = "Draft";

    public List<int> TagIds { get; set; } = new();
}

public class ArticleUpdateDto : ArticleCreateDto
{
    public int ArticleID { get; set; }
}

public class CommentDto
{
    public int CommentID { get; set; }
    public int ArticleID { get; set; }
    public string Author { get; set; } = string.Empty;
    public int UserID { get; set; }

    [Required(ErrorMessage = "Comment cannot be empty.")]
    [MaxLength(1000)]
    public string CommentText { get; set; } = string.Empty;
    public DateTime CommentDate { get; set; }
}

public class ArticleSearchFilterDto
{
    public string? Keyword { get; set; }
    public string? SearchColumn { get; set; } = "Title"; // Title, Content
    public int? CategoryId { get; set; }
    public int? TagId { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "Newest"; // Newest, Trending, MostViewed
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
