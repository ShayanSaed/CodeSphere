using CodeSphere.Core.Data;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using CodeSphere.Core.Services;
using CodeSphere.Web.Middleware;
using CodeSphere.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Database (EF Core, code-first) ----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<CodeSphereDbContext>(options =>
    options.UseSqlServer(connectionString));

// The built-in Identity UI pages (Register, ForgotPassword, ResendEmailConfirmation, ...)
// require an IEmailSender in their constructor. Without *some* registered implementation,
// DI throws "Unable to resolve service for type 'IEmailSender'" the instant any of those
// pages is requested — which was the root cause of Register.cshtml failing to load.
builder.Services.AddSingleton<IEmailSender, CodeSphere.Web.Services.NoOpEmailSender>();

// ---------------- ASP.NET Core Identity (bonus: authentication + roles) ----------------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<CodeSphereDbContext>();

// AJAX (fetch) calls from site.js send the antiforgery token via this header
// instead of a form field, since the request bodies are JSON.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ---------------- Dependency Injection: business services behind interfaces (requirement #6) ----------------
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddRazorPages(options =>
{
    // Every registered account may write articles — dev.to-style platforms don't
    // gate authorship behind a separate "Author" role, so this only requires the
    // user to be signed in (any authenticated Reader or Admin).
    options.Conventions.AuthorizeFolder("/Articles/Manage");
    options.Conventions.AuthorizeFolder("/Dashboard", "AdminOnly");
});

var app = builder.Build();

// ---------------- Seed database (roles, admin, sample data) ----------------
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// ---------------- Middleware pipeline ----------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCodeSphereRequestLogging();   // custom middleware #1
app.UseCodeSphereExceptionHandling(); // custom middleware #2

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
