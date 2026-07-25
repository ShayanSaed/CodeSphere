# CodeSphere — Developer Blogging Platform
### Final Project — Web Programming Course

CodeSphere is a dev.to-style platform: every registered user can write
articles, tag and categorize them, react and comment, follow each other,
bookmark posts, and maintain a public profile. This solution implements the
platform end-to-end on ASP.NET Core 8, built around the schema originally
sketched in `CodeSphere.sql`.

## Solution structure

```
CodeSphere.sln
src/
  CodeSphere.Core/     Class library: entities, EF Core DbContext + migrations,
                       DTOs, service interfaces, service implementations
                       (business logic), PDF/Excel export, demo data
                       generator, custom exceptions.
  CodeSphere.Web/      Razor Pages front-end: browsing, search/filter,
                       article CRUD, comments/reactions/bookmarks (AJAX),
                       public user profiles, admin dashboard (Chart.js),
                       print-ready reports, ASP.NET Core Identity (cookie
                       auth + roles).
  CodeSphere.Api/      REST API: JWT-secured endpoints for every resource,
                       Swagger/OpenAPI documentation, same DbContext/services
                       as the Web project (shared via CodeSphere.Core).
```

Both front-ends sit on the **same** EF Core code-first model and the
**same** service layer in `CodeSphere.Core` — there is exactly one source of
truth for business rules (e.g. "you can only edit your own article unless
you're an Admin" lives in `ArticleService`, not duplicated in a page and a
controller).

## Roles

CodeSphere only has two roles: **Reader** (granted automatically to every
self-registered account) and **Admin**. There is deliberately no separate
"Author" role — any signed-in user may write, edit, and delete their own
articles, which is how a dev.to-style platform is actually supposed to work.
Admins additionally manage categories/tags, moderate content, and see the
admin dashboard.

## How the mandatory requirements map to the code

| # | Requirement | Where |
|---|---|---|
| 1 | ≥6 tables, 2× one-to-many, 1× many-to-many, sample data | `Core/Entities/*.cs`, `Core/Data/CodeSphereDbContext.cs` (Fluent API), `Core/Data/DbSeeder.cs` + `Core/Data/DemoDataSeeder.cs`. Tables: Users (Identity), UserProfiles, Categories, Articles, Comments, Reactions, Tags, ArticleTags, Bookmarks, Follows. One-to-many: User→Articles, Category→Articles (also Article→Comments, Article→Reactions). Many-to-many: Article↔Tag via `ArticleTags`. Sample data: 200+ rows in every table (see below). |
| 2 | EF Core, LINQ, navigation properties, CRUD | Every `*Service.cs` in `Core/Services`. |
| 3 | Razor Pages CRUD | `Web/Pages/Articles/Manage`, `Web/Pages/Categories`, `Web/Pages/Tags`. |
| 4 | Data Annotation validation | Entities and DTOs (`[Required]`, `[MaxLength]`, `[Range]`, `[RegularExpression]`, ...), enforced both client-side (jQuery Validation) and server-side (`ModelState.IsValid`), plus a custom `ImageUrlValidator` for profile photo URLs. |
| 5 | Web API, ≥8 endpoints, Swagger | `Api/Controllers/*.cs` — 34 endpoints across 10 controllers (Articles, Categories, Tags, Comments, Reactions, Bookmarks, Follows, Reports, Auth, Users). Full list with request/response examples in `API_DOCUMENTATION.md`. Swagger UI at `/swagger` in Development. |
| 6 | Dependency Injection, services behind interfaces | `Core/Interfaces/*.cs` + `Core/Services/*.cs`, registered in both `Program.cs` files. |
| 7 | Custom middleware | `Web/Middleware/ExceptionHandlingMiddleware.cs`, `Web/Middleware/RequestLoggingMiddleware.cs`, `Api/Middleware/ApiExceptionHandlingMiddleware.cs`. |
| 8 | Search + filter | `ArticleService.SearchAsync` — keyword search over Title/Content, filter by category/tag/status, sort by newest/trending/most-viewed. |
| 9 | 2 print-ready reports | `Web/Pages/Reports/Trending.cshtml` and `UserActivity.cshtml` — `print.css` hides all chrome and prints a clean table; both also export to PDF/Excel. |
| 10 | Error handling | Custom middleware above + `Pages/Error*.cshtml` friendly pages + `ServiceResult`/`ServiceResult<T>` pattern so services never throw for expected failures. |

## User profiles

Every account (self-registered or seeded) has a `UserProfiles` row, shown on
a public profile page at `/Users/{id}` (`Web/Pages/Users/Index.cshtml`) —
full name, bio, country, website, profile photo, follower/following counts,
and the user's published articles. The same data is available over the API
at `GET /api/users/{id}`.

Profile photos are supplied as a **URL**, collected at registration
(`Areas/Identity/Pages/Account/Register.cshtml`) and validated by
`Core/Common/ImageUrlValidator.cs` before anything is stored: the URL must be
an absolute `http`/`https` link (no `javascript:`/`data:`/other schemes) and
under the database column's length limit. The app never fetches this URL
itself — it is only ever rendered client-side as `<img src="...">` — so
there's no server-side SSRF surface from this field; the validator exists to
stop a malicious value from being persisted in the first place. See the
comments in `ImageUrlValidator.cs` for the full reasoning.

## Sample data

`Core/Data/DemoDataSeeder.cs` generates a large, realistic dataset the first
time the app runs against an empty database — **200+ rows in every table**
defined by the original `CodeSphere.sql` schema:

| Table | Approx. rows |
|---|---|
| Categories | 220 |
| Tags | 228 |
| Users / UserProfiles | ~230 |
| Articles | 225 |
| ArticleTags | ~500+ |
| Comments | 260 |
| Reactions | 260 |
| Bookmarks | 230 |
| Follows | 230 |

Article bodies are assembled from curated technical paragraph templates
(introduction, core concept, practical considerations, pitfalls, conclusion)
combined per-article with a specific technology topic, so they read as
coherent, on-topic technical writing rather than Lorem Ipsum — with enough
combinatorial variety that no two articles read identically.

## Bonus features implemented

- **Authentication & roles** — ASP.NET Core Identity (Admin / Reader) on the Web project, JWT bearer tokens on the API (`POST /api/auth/login`, `POST /api/auth/register`).
- **Public user profiles** — `/Users/{id}` and `GET /api/users/{id}`, including a securely-validated profile photo URL.
- **Pagination** — `PagedResult<T>` used by the article listing (page size 8) and the API's `GET /api/articles`.
- **Charts** — `/Dashboard` (Admin-only) renders Chart.js doughnut/line/bar charts fed by a JSON handler.
- **PDF/Excel export** — `IExportService` (QuestPDF + ClosedXML) — buttons on both report pages, plus API endpoints under `/api/reports/.../export/...`.
- **AJAX / Fetch API** — reactions, bookmarks, inline comments, profile follow/unfollow, and the dashboard charts all use `fetch()` (see `wwwroot/js/site.js`) instead of full page posts.

## Getting started

You'll need the **.NET 8 SDK** and **SQL Server** (LocalDB is fine) — Visual
Studio 2022 (17.8+) with the ASP.NET workload includes both.

1. **Restore packages** (Visual Studio does this automatically on open, or run):
   ```
   dotnet restore
   ```

2. **Set your connection string.** `appsettings.json` in both `CodeSphere.Web`
   and `CodeSphere.Api` points at `(localdb)\mssqllocaldb` by default — change
   `ConnectionStrings:DefaultConnection` in each if you're using a different
   SQL Server instance.

3. **Create the database with EF Core migrations** (code-first, per the
   project's chosen approach). From `src/CodeSphere.Web`:
   ```
   dotnet tool install --global dotnet-ef   # if you don't have it yet
   dotnet ef migrations add InitialCreate --project ../CodeSphere.Core --startup-project .
   dotnet ef database update --project ../CodeSphere.Core --startup-project .
   ```
   This creates every table (Identity + domain) from the `CodeSphereDbContext`
   model. The API project talks to the same database, so you only need to
   run migrations once, from the Web project.

4. **Run both projects.** In Visual Studio, right-click the solution →
   *Configure Startup Projects* → *Multiple startup projects* → set both
   `CodeSphere.Web` and `CodeSphere.Api` to **Start**, then F5. From the CLI,
   run each in its own terminal:
   ```
   dotnet run --project src/CodeSphere.Web
   dotnet run --project src/CodeSphere.Api
   ```

5. **First run** seeds the database automatically: roles, an admin account,
   and the full sample dataset described above (this can take a little while
   the first time — it's creating 200+ user accounts through ASP.NET Core
   Identity, which hashes each password). Default admin login:
   - Email: `admin@codesphere.dev`
   - Password: `Admin@12345`

   All ~230 generated sample accounts share the password `Reader@12345`
   (their emails follow the pattern `{username}@example.com` — see the
   generated `Users`/`UserProfiles` tables, or the User Activity report, for
   usernames).

6. **Swagger** is available at `https://localhost:7101/swagger` once the API
   is running — use `POST /api/auth/login` (or `POST /api/auth/register` to
   create a fresh account) to get a JWT, then click **Authorize** in Swagger
   UI and paste `Bearer <token>`.

## Notes on the SQL-first version

`CodeSphere.sql` (the original SQL Server script with its views and scalar
functions) is kept in the project root purely as a design reference — since
this build uses the **code-first** EF Core approach, the actual schema is
generated by the migrations described above, not by running that script.
The LINQ in `ReportService` deliberately mirrors the logic that used to live
in `TrendingArticlesView`, `UserActivityView`, `GetEngagementScore` and
`GetTotalFollowers`, so the reporting behaviour is unchanged.

## Further documentation

- `PROJECT_REPORT.md` — full project report (requirements, architecture,
  database design, implementation notes, challenges, references).
- `API_DOCUMENTATION.md` — every API endpoint with request/response examples.
