using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

// Composite-key join entity for the many-to-many Article <-> Tag relationship
public class ArticleTag
{
    public int ArticleID { get; set; }
    public int TagID { get; set; }

    [ForeignKey(nameof(ArticleID))]
    public Article? Article { get; set; }

    [ForeignKey(nameof(TagID))]
    public Tag? Tag { get; set; }
}
