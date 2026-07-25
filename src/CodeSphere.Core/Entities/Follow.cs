using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSphere.Core.Entities;

public class Follow
{
    [Key]
    public int FollowID { get; set; }

    [Required]
    public int FollowerUserID { get; set; }

    [Required]
    public int FollowingUserID { get; set; }

    public DateTime FollowDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FollowerUserID))]
    public ApplicationUser? FollowerUser { get; set; }

    [ForeignKey(nameof(FollowingUserID))]
    public ApplicationUser? FollowingUser { get; set; }
}
