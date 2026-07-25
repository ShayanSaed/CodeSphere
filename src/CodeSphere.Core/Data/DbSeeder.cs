using CodeSphere.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeSphere.Core.Data;

/// <summary>
/// Seeds roles and the administrator account on every startup (idempotent),
/// then — on a brand-new, empty database only — hands off to
/// <see cref="DemoDataSeeder"/> to populate a large, realistic sample
/// dataset (categories, tags, users, articles, comments, reactions,
/// bookmarks and follows), so the app is demo-ready right after
/// `dotnet ef database update`.
///
/// Only two roles exist: "Admin" and "Reader". There is deliberately no
/// separate "Author" role — every registered account may write articles,
/// which matches how a dev.to-style platform actually works; "Author" would
/// have been an artificial gate with no real purpose here.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<CodeSphereDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        await context.Database.MigrateAsync();

        // ---------------- Roles ----------------
        foreach (var role in new[] { "Admin", "Reader" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        // ---------------- Admin user (+ profile, so every account has one) ----------------
        var admin = await userManager.FindByEmailAsync("admin@codesphere.dev");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@codesphere.dev",
                EmailConfirmed = true,
                Status = "Active",
                JoinDate = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin@12345");
            await userManager.AddToRoleAsync(admin, "Admin");

            context.UserProfiles.Add(new UserProfile
            {
                UserID = admin.Id,
                FullName = "CodeSphere Admin",
                Bio = "Platform administrator for CodeSphere.",
                Country = "N/A",
                ProfileImageURL = "https://ui-avatars.com/api/?name=Code+Sphere&background=2b3a67&color=fff&size=256"
            });
            await context.SaveChangesAsync();
        }

        // ---------------- Large sample dataset ----------------
        // Guards against re-running on a database that already has the full
        // dataset, while still topping up a database that only has the old,
        // much smaller hand-written seed (a handful of categories/articles
        // from an earlier version of this project) — the threshold is well
        // below the 200+ rows this seeder aims for, and well above what any
        // hand-written seed produced, so it reliably tells the two apart.
        // SeedCategoriesAsync/SeedTagsAsync are themselves defensive about
        // not re-inserting names that already exist, so running this against
        // a partially-seeded database is safe.
        if (await context.Articles.CountAsync() < 50)
        {
            await DemoDataSeeder.SeedAsync(context, userManager);
        }
    }
}
