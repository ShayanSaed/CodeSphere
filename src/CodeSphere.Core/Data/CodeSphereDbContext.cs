using CodeSphere.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Data;

/// <summary>
/// Code-first DbContext that recreates the CodeSphere schema (originally
/// authored in CodeSphere.sql) via EF Core migrations. Identity tables
/// (AspNetUsers, AspNetRoles, ...) replace the standalone Users table;
/// ApplicationUser carries the extra JoinDate/Status columns.
/// </summary>
public class CodeSphereDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public CodeSphereDbContext(DbContextOptions<CodeSphereDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---------------- ApplicationUser ----------------
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.Status).HasDefaultValue("Active");
            b.Property(u => u.JoinDate).HasDefaultValueSql("GETDATE()");
        });

        // ---------------- UserProfile (1-1 with User) ----------------
        builder.Entity<UserProfile>(b =>
        {
            b.HasIndex(p => p.UserID).IsUnique(); // UQ_UserProfiles_UserID
            b.HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Category ----------------
        builder.Entity<Category>(b =>
        {
            b.HasIndex(c => c.CategoryName).IsUnique(); // UQ_Categories_CategoryName
        });

        // ---------------- Tag ----------------
        builder.Entity<Tag>(b =>
        {
            b.HasIndex(t => t.TagName).IsUnique(); // UQ_Tags_TagName
        });

        // ---------------- Article (1-many: User->Articles, Category->Articles) ----------------
        builder.Entity<Article>(b =>
        {
            b.Property(a => a.ViewCount).HasDefaultValue(0);
            b.Property(a => a.ReadingTime).HasDefaultValue(1);
            b.Property(a => a.Status).HasDefaultValue("Published");
            b.Property(a => a.PublishDate).HasDefaultValueSql("GETDATE()");

            b.HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(a => a.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(a => a.CategoryID);
            b.HasIndex(a => a.UserID);
            b.HasIndex(a => new { a.Status, a.PublishDate });

            b.ToTable(t => t.HasCheckConstraint("CK_Articles_ReadingTime", "[ReadingTime] > 0"));
            b.ToTable(t => t.HasCheckConstraint("CK_Articles_ViewCount", "[ViewCount] >= 0"));
            b.ToTable(t => t.HasCheckConstraint("CK_Articles_Status", "[Status] = 'Draft' OR [Status] = 'Published'"));
        });

        // ---------------- Comment (1-many: Article->Comments, User->Comments) ----------------
        builder.Entity<Comment>(b =>
        {
            b.Property(c => c.CommentDate).HasDefaultValueSql("GETDATE()");

            b.HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleID)
                .OnDelete(DeleteBehavior.Cascade); // FK_Comments_Articles ON DELETE CASCADE

            b.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(c => c.ArticleID);
        });

        // ---------------- Reaction ----------------
        builder.Entity<Reaction>(b =>
        {
            b.Property(r => r.ReactionType).HasDefaultValue("Like");
            b.Property(r => r.ReactionDate).HasDefaultValueSql("GETDATE()");

            b.HasOne(r => r.Article)
                .WithMany(a => a.Reactions)
                .HasForeignKey(r => r.ArticleID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(r => r.ArticleID);
            b.ToTable(t => t.HasCheckConstraint("CK_Reactions_ReactionType", "[ReactionType] IN ('Idea','Love','Like')"));
        });

        // ---------------- ArticleTag (many-to-many) ----------------
        builder.Entity<ArticleTag>(b =>
        {
            b.HasKey(at => new { at.ArticleID, at.TagID });

            b.HasOne(at => at.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(at => at.ArticleID)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(at => at.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(at => at.TagID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Bookmark ----------------
        builder.Entity<Bookmark>(b =>
        {
            b.Property(bm => bm.SavedDate).HasDefaultValueSql("GETDATE()");

            b.HasOne(bm => bm.Article)
                .WithMany(a => a.Bookmarks)
                .HasForeignKey(bm => bm.ArticleID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(bm => bm.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(bm => bm.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(bm => bm.UserID);
        });

        // ---------------- Follow (self-referencing many-to-many on User) ----------------
        builder.Entity<Follow>(b =>
        {
            b.Property(f => f.FollowDate).HasDefaultValueSql("GETDATE()");

            b.HasOne(f => f.FollowerUser)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerUserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(f => f.FollowingUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingUserID)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(f => new { f.FollowerUserID, f.FollowingUserID }).IsUnique(); // UQ_Follows
            b.HasIndex(f => f.FollowingUserID);

            b.ToTable(t => t.HasCheckConstraint("CK_Follows_FollowerUserID", "[FollowerUserID] <> [FollowingUserID]"));
        });
    }
}
