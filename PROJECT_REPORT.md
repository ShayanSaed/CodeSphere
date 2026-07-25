# CodeSphere — Developer Blogging Platform
## Project Report

**Course:** Web Programming — Final Project
**Project Type:** ASP.NET Core 8 Web Application (Razor Pages + REST API)
**Repository:** `ShayanSaed/CodeSphere`

---

## Abstract

CodeSphere is a dev.to-style developer blogging platform that allows every
registered user to publish technical articles, organize them by category and
tag, react to and comment on each other's work, follow other authors,
maintain a public profile, and bookmark articles for later reading. The
system is implemented on ASP.NET Core 8 using a three-project, layered
architecture (`CodeSphere.Core`, `CodeSphere.Web`, `CodeSphere.Api`) built
around a single Entity Framework Core code-first data model. The platform
satisfies the course's mandatory requirements — a relational schema with
one-to-many and many-to-many relationships, EF Core CRUD via LINQ, Razor
Pages CRUD, server-side validation, a documented REST API, dependency-
injected services, custom middleware, search/filtering, two print-ready
reports, and centralized error handling — and extends them with
authentication, public user profiles, pagination, an administrative
analytics dashboard with charts, PDF/Excel export, and an AJAX/Fetch-driven
interaction layer for comments, reactions, and bookmarks. The database is
seeded with a large, realistic sample dataset (200+ rows in every table) so
these capabilities are actually visible in a fresh install rather than only
on paper.

A subsequent usability and responsive-design pass — prompted directly by
what that realistic data volume exposed — added a searchable users
directory, a compact windowed pagination control shared across every list
page, advanced search on the "My Articles" and "Bookmarks" pages (not just
the home page), an intellisense-style tag picker, sticky report table
headers that become self-contained scrollboxes on mobile, toast-style error
notifications in place of browser `alert()`s, a sticky navigation bar, and a
light/dark theme toggle.

Consistent with how a dev.to-style platform actually works, there is no
separate "Author" role gating who may publish — every registered account can
write, edit, and delete their own articles.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Functional Requirements](#2-functional-requirements)
3. [Non-Functional Requirements](#3-non-functional-requirements)
4. [System Architecture](#4-system-architecture)
5. [Database Design](#5-database-design)
6. [Implementation](#6-implementation)
7. [Web API](#7-web-api)
8. [Testing and Quality Assurance](#8-testing-and-quality-assurance)
9. [Error Handling Strategy](#9-error-handling-strategy)
10. [Challenges and Solutions](#10-challenges-and-solutions)
11. [Future Enhancements](#11-future-enhancements)
12. [Conclusion](#12-conclusion)
13. [References](#13-references)

### List of Figures

> The figures below are referenced throughout the report at the point where
> they are most relevant. Insert each image at its marked location; the
> captions and figure numbers are already set so the table of contents and
> in-text references stay consistent.

| Figure | Description | Section |
|---|---|---|
| Figure 1 | Entity-Relationship (ER) diagram of the CodeSphere database | 5.3 |
| Figure 2 | Home page — article discovery, search and filtering | 6.5 |
| Figure 3 | Article details page — icon-based reactions, bookmark, comments | 6.5 |
| Figure 4 | Article create/edit form with validation messages | 6.6 |
| Figure 5 | Login page | 6.10 |
| Figure 6 | Register page — account fields and optional profile fields | 6.10 |
| Figure 7 | Admin dashboard with charts | 6.11 |
| Figure 8 | Trending Articles report (print view) | 6.9 |
| Figure 9 | User Activity report (print view) | 6.9 |
| Figure 10 | Exported PDF report | 6.9 |
| Figure 11 | Exported Excel report | 6.9 |
| Figure 12 | Swagger UI — API documentation | 7.4 |
| Figure 13 | Swagger UI — JWT authorization dialog | 7.4 |
| Figure 14 | Public user profile page — profile photo, bio, published articles | 6.13 |
| Figure 15 | Users directory page — searchable list of all users | 6.14 |
| Figure 16 | Tag picker — intellisense-style tag search with chips, on the Create Article form | 6.14 |
| Figure 17 | Compact pagination ("1 2 3 … 27 28 29") on the home page | 6.14 |
| Figure 18 | Reports page on mobile — stacked action buttons, self-contained scrollable table | 6.14 |
| Figure 19 | Toast-style error notification (bottom corner) | 6.14 |
| Figure 20 | Dark theme, toggled from the navbar | 6.14 |

---

## 1. Introduction

### 1.1 Project Overview

CodeSphere is a community platform for developers to write and share
technical articles, similar in spirit to dev.to or Medium. Every registered
user publishes articles under a category, tags them with relevant
technologies, and readers can react (Like / Love / Idea), comment, bookmark
articles for later, and follow the authors they find valuable. Every account
has a public profile page showing who they are and what they've published.
The system was designed from an initial SQL Server schema (`CodeSphere.sql`)
and re-implemented as an EF Core code-first model so that the database,
business logic, and both front-ends (a Razor Pages web application and a
REST API) share a single, consistent source of truth.

### 1.2 Objectives

The project set out to demonstrate, in a single cohesive application, the
full set of concepts covered during the semester:

- Relational database design with properly normalized tables and constraints.
- Data access through an ORM (EF Core) using LINQ and navigation properties.
- A complete CRUD experience through server-rendered Razor Pages.
- A documented, versioned REST API consumable by third-party clients.
- Enforced business rules through a dependency-injected service layer, kept
  independent of any specific UI technology.
- Defensive engineering practices: server-side validation, centralized
  exception handling, and custom middleware.
- Practical, everyday platform features: search, filtering, pagination,
  reporting, and data export.
- A realistic, sufficiently large sample dataset so every feature above is
  actually observable, not just theoretically implemented.

### 1.3 Scope

The current release covers the full content lifecycle (draft → published),
social interaction (comments, reactions, follows, bookmarks), taxonomy
management (categories, tags), authentication (Reader / Admin), public user
profiles with a photo, and two administrative reports available as
on-screen tables, print-ready pages, PDF, and Excel. Out of scope for this
release: real email delivery (a no-op email sender is used, see §10),
external OAuth login providers, actual image hosting/upload for profile
photos (a URL is supplied instead — see §6.13), and a dedicated "edit
profile after registration" page (see §11).

---

## 2. Functional Requirements

### 2.1 Actors

| Actor | Description |
|---|---|
| **Anonymous visitor** | Can browse, search, and read published articles, categories, tags, public user profiles, and both reports. Cannot comment, react, bookmark, follow, or publish. |
| **Reader** | The single role granted automatically on self-registration. Has full read access, plus commenting, reacting, bookmarking, following — **and publishing, editing, and deleting their own articles.** There is deliberately no separate "Author" role: on a platform whose entire premise is that anyone can write, gating authorship behind an extra role would be an artificial restriction with no real purpose. |
| **Admin** | Everything a Reader can do, plus: manage categories and tags, moderate/delete any article or comment, and view the administrative dashboard. |

### 2.2 Functional Requirements List

| ID | Requirement | Actor(s) |
|---|---|---|
| FR-01 | Users can register an account (optionally supplying profile details and a photo URL) and log in / log out. | Reader |
| FR-02 | Any authenticated user can create, edit, and delete their own articles (drafts or published) — publishing is not gated behind a separate role. | Reader, Admin |
| FR-03 | Visitors can browse published articles with keyword search (title or content), category filter, tag filter, and sorting (newest, trending, most viewed). | All |
| FR-04 | Article listings are paginated (20 per page on the home page), using a compact "1 2 3 … N-1 N" pager with Previous/Next controls that stays usable on narrow screens. | All |
| FR-05 | Users can comment on an article and delete their own comments. | Reader, Admin |
| FR-06 | Users can react to an article with Like, Love, or Idea, shown as icons rather than text (one active reaction per user per article, toggled on/off). | Reader, Admin |
| FR-07 | Users can bookmark/unbookmark an article, and search/filter/sort/paginate within their own bookmarked articles using the same advanced search controls as the home page. | Reader, Admin |
| FR-08 | Users can follow/unfollow other users and see follower/following counts. | Reader, Admin |
| FR-09 | Every user has a public profile page showing their `UserProfiles` data (full name, bio, country, website, photo) and their published articles; a searchable directory page lists all users. | All (view), Reader/Admin (own profile is created at registration) |
| FR-09a | Authors can search/filter/sort/paginate within their own articles (drafts included) on the "My Articles" page, using the same advanced search controls as the home page, plus a Draft/Published status filter. | Reader, Admin |
| FR-09b | Admins can search Categories and Tags by name. | Admin (write), All (browse/search) |
| FR-09c | When adding tags to an article, the author searches for tags by name (an intellisense-style picker) and adds/removes them as chips, rather than scanning a long checkbox list. | Reader, Admin |
| FR-10 | Admins can create, edit, and delete categories and tags. | Admin |
| FR-11 | The system prevents deleting a category that still has articles assigned to it. | Admin |
| FR-12 | The system exposes a Trending Articles report (ranked by an engagement score) and a User Activity report (articles/comments/reactions/followers per user), both open to every visitor. | All |
| FR-13 | Both reports can be printed (print-optimized layout) and exported to PDF and Excel; column headers stay visible while scrolling, and on mobile each table is a self-contained, vertically scrollable box that never pushes the page itself out of frame. | All |
| FR-14 | Admins can view a dashboard with aggregate statistics and charts (articles by category, articles published over time, top 5 trending articles). | Admin |
| FR-15 | All the above capabilities are also exposed through a versioned REST API secured with JWT bearer tokens, including registration. | External clients |
| FR-16 | The system validates all user input server-side (in addition to client-side hints) and rejects invalid submissions with actionable error messages — including a dedicated security check on profile photo URLs. | All |
| FR-17 | The system presents friendly error pages for not-found, forbidden, and bad-request conditions instead of raw stack traces; client-side (AJAX) failures surface as a dismissable toast notification in the corner of the screen rather than a browser `alert()` dialog. | All |
| FR-18 | The navigation bar stays visible at the top of the viewport while scrolling. | All |
| FR-19 | Users can switch between a light and dark visual theme from the navbar; the choice persists across visits. | All |

---

## 3. Non-Functional Requirements

| Category | Requirement |
|---|---|
| **Maintainability** | Business logic lives in a single shared class library (`CodeSphere.Core`) behind interfaces, so the Razor Pages front-end and the REST API cannot drift into duplicated or inconsistent rules. |
| **Security** | Passwords are hashed by ASP.NET Core Identity (never stored in plain text); the API uses signed, time-limited JWTs; administrative actions are protected by role-based authorization policies; anti-forgery tokens protect state-changing Razor Pages requests, including AJAX calls; user-supplied profile photo URLs are validated against a scheme allow-list before being persisted (§6.13). |
| **Usability** | Responsive Bootstrap 5 layout; inline AJAX interactions (comments, reactions, bookmarks, follows) avoid full-page reloads; validation errors are shown next to the relevant field; reactions are represented with icons instead of raw text labels. |
| **Performance** | List endpoints are paginated server-side; EF Core queries use `AsNoTracking()` for read-only listings and `Include`/`ThenInclude` to avoid N+1 query patterns. |
| **Extensibility** | New report types can be added by implementing `IReportService`/`IExportService`; new resources follow the existing Interface → Service → Controller/Page pattern. |
| **Portability** | Runs on any platform supported by .NET 8 and SQL Server (including SQL Server LocalDB for local development). |
| **Data realism** | The seeded sample dataset provides 200+ rows in every table, with coherent, technically-themed article content, so search, filtering, pagination, and the reports are all meaningfully exercised out of the box. |

---

## 4. System Architecture

### 4.1 High-Level Architecture

CodeSphere is split into three .NET projects inside a single solution:

- **`CodeSphere.Core`** — a class library with no UI dependencies. It contains
  the EF Core entities, the `CodeSphereDbContext`, all DTOs, the service
  interfaces and their implementations (business logic), the PDF/Excel
  export logic, the profile-photo URL validator, and the database/demo-data
  seeders.
- **`CodeSphere.Web`** — an ASP.NET Core Razor Pages application. It renders
  the public site and the authenticated user/admin experience, hosts
  ASP.NET Core Identity (cookie authentication, roles), and exposes small
  AJAX page handlers for comments, reactions, bookmarks, follows, and the
  dashboard's chart data.
- **`CodeSphere.Api`** — an ASP.NET Core Web API project exposing the same
  domain operations as a documented, JWT-secured REST API for external
  clients, using Swagger/OpenAPI for interactive documentation.

Both `CodeSphere.Web` and `CodeSphere.Api` reference `CodeSphere.Core` and
therefore share the exact same `DbContext`, entities, and service layer —
there is one implementation of every business rule, not two.

```
                        ┌───────────────────────────┐
                        │        SQL Server          │
                        │   (CodeSphere database)    │
                        └──────────────┬─────────────┘
                                       │ EF Core
                        ┌──────────────▼─────────────┐
                        │      CodeSphere.Core        │
                        │  Entities · DbContext        │
                        │  DTOs · Interfaces · Services │
                        │  PDF/Excel export · Validators │
                        │  Seeder · Demo Data Generator   │
                        └──────┬────────────────┬──────┘
                               │                │
                 ┌─────────────▼───┐      ┌─────▼─────────────┐
                 │  CodeSphere.Web  │      │  CodeSphere.Api    │
                 │  Razor Pages      │      │  REST Controllers   │
                 │  ASP.NET Identity │      │  JWT Bearer Auth     │
                 │  (cookie auth)    │      │  Swagger / OpenAPI   │
                 └───────┬──────────┘      └─────────┬──────────┘
                         │                            │
                 ┌───────▼──────────┐        ┌────────▼─────────┐
                 │  Browser (users)  │        │  External clients │
                 │  Bootstrap + Fetch│        │  (Swagger UI, etc.)│
                 └───────────────────┘        └───────────────────┘
```

### 4.2 Layering Within `CodeSphere.Core`

Within the shared library, responsibilities are kept in strict layers:

1. **Entities** (`Entities/*.cs`) — persistence-mapped classes with Data
   Annotation validation attributes.
2. **Data** (`Data/CodeSphereDbContext.cs`, `Data/DbSeeder.cs`,
   `Data/DemoDataSeeder.cs`) — EF Core Fluent API configuration (keys,
   relationships, constraints, defaults), role/admin seeding, and the
   large-scale sample-data generator.
3. **DTOs** (`DTOs/*.cs`) — the shapes actually sent to and received from
   clients; they never leak EF Core navigation properties or tracking state.
4. **Common** (`Common/*.cs`) — cross-cutting helpers shared by both
   front-ends: `PagedResult<T>`, `ServiceResult`/`ServiceResult<T>`, custom
   exceptions, and `ImageUrlValidator`.
5. **Interfaces** (`Interfaces/*.cs`) — the public contract of each service
   (`IArticleService`, `ICommentService`, `IReactionService`,
   `ICategoryService`, `ITagService`, `IBookmarkService`, `IFollowService`,
   `IReportService`, `IExportService`, `IUserService`).
6. **Services** (`Services/*.cs`) — the actual business logic: authorization
   checks (e.g. "only the owner or an Admin may edit this article"),
   validation beyond Data Annotations (e.g. duplicate category names,
   profile photo URL safety), and all EF Core/LINQ queries.

Both `CodeSphere.Web` and `CodeSphere.Api` register these services purely
through their interfaces via the built-in ASP.NET Core dependency injection
container (`AddScoped<IArticleService, ArticleService>()`, etc.), so either
front-end could be replaced without touching business logic.

---

## 5. Database Design

### 5.1 Entity Overview

| Table | Purpose |
|---|---|
| `AspNetUsers` (`ApplicationUser`) | Registered users. Extends ASP.NET Core Identity's user with `JoinDate` and `Status` (Active/Deactive). |
| `AspNetRoles` / `AspNetUserRoles` | Identity role tables — only `Admin` and `Reader` exist. |
| `UserProfiles` | One-to-one extension of a user: full name, bio, country, website, avatar URL. Populated for every account (at registration, or by the seeder) and surfaced on the public profile page (§6.13) — this table is no longer defined-but-unused. |
| `Categories` | Article categories (e.g. "Database Engineering — Performance Optimization"). Unique name. |
| `Articles` | The core content entity: title, content, status (Draft/Published), view count, reading time, publish date. |
| `Comments` | Comments left by a user on an article. |
| `Reactions` | A user's reaction (Like/Love/Idea) to an article. |
| `Tags` | Free-form topical tags (e.g. "SQL", "Docker"). Unique name. |
| `ArticleTags` | Join table implementing the Article ↔ Tag many-to-many relationship. |
| `Bookmarks` | A user's saved articles. |
| `Follows` | A self-referencing many-to-many relationship on `ApplicationUser` (follower/following). |

### 5.2 Relationships

- **One-to-many:** `ApplicationUser` → `Articles` (an author writes many
  articles); `Category` → `Articles`; `Article` → `Comments`; `Article` →
  `Reactions`; `ApplicationUser` → `Comments`/`Reactions`/`Bookmarks`.
- **One-to-one:** `ApplicationUser` ↔ `UserProfile`.
- **Many-to-many:** `Article` ↔ `Tag` through `ArticleTags`; and a
  self-referencing many-to-many on `ApplicationUser` through `Follows`
  (`FollowerUserID` → `FollowingUserID`).

### 5.3 Entity-Relationship Diagram

> **Figure 1 — Entity-Relationship diagram of the CodeSphere database.**
>
> _Insert the ER diagram image here._
>
> `![Figure 1: CodeSphere ER Diagram](images/er-diagram.png)`

### 5.4 Schema Summary

Key constraints enforced at the database level (mirrored from the original
`CodeSphere.sql` design and re-expressed through EF Core's Fluent API in
`CodeSphereDbContext`):

| Constraint | Table | Rule |
|---|---|---|
| `CK_Articles_Status` | Articles | `Status` must be `'Draft'` or `'Published'` |
| `CK_Articles_ViewCount` | Articles | `ViewCount >= 0` |
| `CK_Articles_ReadingTime` | Articles | `ReadingTime > 0` |
| `CK_Reactions_ReactionType` | Reactions | `ReactionType` in `('Like','Love','Idea')` |
| `CK_Follows_FollowerUserID` | Follows | `FollowerUserID <> FollowingUserID` (a user cannot follow themselves) |
| `UQ_Categories_CategoryName` | Categories | Unique category name |
| `UQ_Tags_TagName` | Tags | Unique tag name |
| `UQ_UserProfiles_UserID` | UserProfiles | One profile per user |
| `UQ_Follows` | Follows | Unique `(FollowerUserID, FollowingUserID)` pair |
| `FK_Comments_Articles` (Cascade) | Comments | Deleting an article deletes its comments |
| `FK_ArticleTags_*` (Cascade) | ArticleTags | Deleting an article or tag removes the association rows |

The database is created and versioned through EF Core migrations generated
from the code-first model (`Core/Data/CodeSphereDbContext.cs`), so the schema
in production is always derived directly from the entity classes rather than
maintained by hand in SQL.

### 5.5 Sample Data Volume

`Core/Data/DemoDataSeeder.cs` populates every table defined in the original
`CodeSphere.sql` schema with at least 200 rows the first time the
application runs against an empty database:

| Table | Rows generated |
|---|---|
| Categories | 220 (20 domains × 11 subtopics) |
| Tags | 228 distinct, real technology names |
| Users / UserProfiles | ~230 accounts (each with a full profile) |
| Articles | 225 |
| ArticleTags | ~500+ (1–4 tags per article) |
| Comments | 260 |
| Reactions | 260 |
| Bookmarks | 230 |
| Follows | 230 |

Article bodies are not Lorem Ipsum: each is assembled from five sections
(introduction, core concept, practical considerations, pitfalls/best
practices, conclusion), each section chosen from a pool of 6–8 hand-written,
topic-parameterized paragraph templates and substituted with one of 75
real technology subjects (ASP.NET Core, Kubernetes, PostgreSQL, and so on).
With 8×8×8×8×6 ≈ 24,600 possible section combinations per topic, the 225
generated articles read as distinct, coherent, technically appropriate
write-ups rather than repeated placeholder text. See §10 for more on this
design decision.

---

## 6. Implementation

### 6.1 Technology Stack

| Layer | Technology |
|---|---|
| Runtime / Framework | .NET 8, ASP.NET Core 8 |
| Data access | Entity Framework Core 8 (code-first, SQL Server provider) |
| Front-end (server-rendered) | Razor Pages, Bootstrap 5, Bootstrap Icons |
| Front-end interactivity | Vanilla JavaScript using the Fetch API, Chart.js |
| Authentication | ASP.NET Core Identity (cookie auth, Reader/Admin roles) for the web app; JWT Bearer for the API |
| API documentation | Swashbuckle (Swagger / OpenAPI 3) |
| PDF export | QuestPDF |
| Excel export | ClosedXML |
| Database | Microsoft SQL Server / LocalDB |

### 6.2 Project Structure

```
CodeSphere.sln
src/
  CodeSphere.Core/
    Entities/        Article, Comment, Reaction, Category, Tag, ArticleTag,
                      Bookmark, Follow, UserProfile, ApplicationUser
    Data/             CodeSphereDbContext, DbSeeder, DemoDataSeeder, Migrations
    DTOs/             Request/response shapes, incl. UserProfileDto
    Interfaces/       Service contracts, incl. IUserService
    Services/         Business logic implementations, incl. UserService
    Common/           PagedResult<T>, ServiceResult/ServiceResult<T>,
                       exceptions, ImageUrlValidator
  CodeSphere.Web/
    Pages/            Razor Pages (Index, Articles, Categories, Tags,
                       Reports, Dashboard, Bookmarks, Users, Error pages)
    Areas/Identity/   Login, Register (incl. profile fields), Logout,
                       Access Denied
    Middleware/       Request logging, exception handling
    wwwroot/          site.css, print.css, site.js
  CodeSphere.Api/
    Controllers/      Articles, Categories, Tags, Comments, Reactions,
                       Bookmarks, Follows, Reports, Auth, Users
    Middleware/        API exception handling (ProblemDetails JSON)
```

### 6.3 Entity Framework Core and Data Access

All persistence goes through `CodeSphereDbContext`, an
`IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`. Relationships,
default values, indexes, and check constraints are configured explicitly in
`OnModelCreating` using the Fluent API rather than relying purely on
convention, which keeps the generated schema an accurate match for the
original database design. Every service method uses asynchronous LINQ
queries (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`) and
navigation properties (`Include`/`ThenInclude`) rather than raw SQL, and
read-only listings are queried with `AsNoTracking()` to reduce overhead.

### 6.4 Service Layer and Dependency Injection

Every piece of business logic is exposed through an interface and registered
with the built-in DI container as a scoped service, e.g.:

```csharp
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
```

Both `CodeSphere.Web`'s `Program.cs` and `CodeSphere.Api`'s `Program.cs`
register the identical set of services against the identical interfaces —
Razor Pages and API controllers are simply two different callers of the same
business logic. Authorization rules that depend on ownership (e.g. "you may
only edit or delete your own article unless you are an Admin") live inside
`ArticleService`/`CommentService`, not duplicated in a page handler and a
controller.

### 6.5 Razor Pages — CRUD and Browsing

The home page (`Pages/Index.cshtml`) lists published articles with search,
category/tag filters, and sorting, and is paginated; each author's name
links to their public profile page. `Pages/Articles/Details` shows the full
article with its tags, an icon-based reaction breakdown, and a comment
thread whose authors also link to their profiles. `Pages/Articles/Manage/*`
provides full CRUD for a user's own articles (Create, Edit, Delete, and a
"My Articles" index) — open to any authenticated user (§6.10).
`Pages/Categories/*` and `Pages/Tags/*` provide the equivalent CRUD for
taxonomy management, restricted to Admins.

> **Figure 2 — Home page (article discovery, search and filtering).**
> _Insert screenshot here._ `![Figure 2: Home page](images/home.png)`

> **Figure 3 — Article details page (icon-based reactions, bookmark, comments).**
> _Insert screenshot here._ `![Figure 3: Article details](images/article-details.png)`

### 6.6 Server-Side Validation

Every entity and DTO carries Data Annotations (`[Required]`, `[MaxLength]`,
`[MinLength]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]`,
`[Compare]`, etc.). Razor Pages checks `ModelState.IsValid` before touching
the database and re-displays the form with `asp-validation-for` messages next
to each invalid field on both the client (via jQuery Validation/unobtrusive
validation) and the server. The Web API performs the same check and returns
`400 Bad Request` with a `ValidationProblemDetails` payload via
`ValidationProblem(ModelState)`. Profile photo URLs go through an additional,
custom check beyond Data Annotations — see §6.13.

> **Figure 4 — Article create/edit form showing validation messages.**
> _Insert screenshot here._ `![Figure 4: Validation](images/validation.png)`

### 6.7 Custom Middleware

Three custom middleware components satisfy the "at least one custom
middleware" requirement (with room to spare):

1. **`RequestLoggingMiddleware`** (`CodeSphere.Web`) — times every request
   and logs write operations (`POST`/`PUT`/`DELETE`) at an "AUDIT" log level
   with the acting user, giving a lightweight audit trail.
2. **`ExceptionHandlingMiddleware`** (`CodeSphere.Web`) — converts known
   domain exceptions (`NotFoundException`, `ForbiddenActionException`,
   `BusinessRuleException`) into friendly redirects to
   `/Error/NotFound`, `/Error/Forbidden`, and `/Error/BadRequest`
   respectively. In `Development`, truly unexpected exceptions are rethrown
   so the built-in Developer Exception Page can show the real stack trace
   rather than being silently masked; in other environments they are logged
   and redirected to a generic `/Error` page. It also checks
   `HttpContext.Response.HasStarted` before attempting a redirect, so it
   never turns one error into a more confusing second one.
3. **`ApiExceptionHandlingMiddleware`** (`CodeSphere.Api`) — the API
   equivalent, translating the same domain exceptions (plus any unhandled
   exception) into RFC 7807 `ProblemDetails` JSON responses with the
   appropriate HTTP status code.

### 6.8 Search and Filtering

`ArticleService.SearchAsync` implements the "at least one search and one
filtering feature" requirement in a single, composable LINQ pipeline:
keyword search over `Title` or `Content` (`EF.Functions.Like`), an optional
category filter, an optional tag filter (`ArticleTags.Any(...)`), an optional
status filter, and a choice of three sort orders (Newest, Trending — by
computed engagement score, Most Viewed). The same filter DTO
(`ArticleSearchFilterDto`) is bound from the Razor Pages query string and
from the API's `[FromQuery]` parameters, so both front-ends expose identical
search behaviour. With 225 seeded articles across 220 categories and 228
tags, these filters now have a realistic amount of data to operate over.

### 6.9 Reporting and Export

Two print-ready reports satisfy the reporting requirement, and both are open
to every visitor (no role restriction on either the Razor Page or the
corresponding API endpoints — see §7.3):

- **Trending Articles** (`Pages/Reports/Trending.cshtml`) — ranked by an
  engagement score (`views + comments × 3 + reactions × 2`), reproducing the
  logic that originally lived in the `TrendingArticlesView` SQL view. Titles
  link through to the article; authors link through to their profile.
- **User Activity** (`Pages/Reports/UserActivity.cshtml`) — total articles,
  comments, reactions, and followers per user, reproducing the
  `UserActivityView` SQL view. Usernames link to the corresponding profile
  page.

Both pages include a *Print* button (a dedicated `print.css` hides all
navigation chrome and renders a clean table) and *PDF*/*Excel* export buttons,
implemented once in `IExportService` (QuestPDF for PDF, ClosedXML for Excel)
and reused by both the Razor Pages report pages' handlers and the API's
`/api/reports/.../export/...` endpoints.

> **Figure 8 — Trending Articles report (print view).**
> _Insert screenshot here._ `![Figure 8: Trending report](images/report-trending.png)`

> **Figure 9 — User Activity report (print view).**
> _Insert screenshot here._ `![Figure 9: User Activity report](images/report-user-activity.png)`

> **Figure 10 — Exported PDF report.**
> _Insert screenshot here._ `![Figure 10: PDF export](images/export-pdf.png)`

> **Figure 11 — Exported Excel report.**
> _Insert screenshot here._ `![Figure 11: Excel export](images/export-excel.png)`

### 6.10 Authentication and Authorization

The web application uses ASP.NET Core Identity with cookie authentication and
exactly two roles: **Reader** (granted automatically to every self-registered
account) and **Admin**. There is no "Author" role — every authenticated user
may write, edit, and delete their own articles; a separate authorship role
would have been an artificial gate on a platform whose entire premise is
open publishing. Registration and login are implemented as custom Razor
Pages under `Areas/Identity/Pages/Account` (rather than the framework's
default scaffolded UI) so that:

- New accounts are looked up and signed in **by e-mail** correctly (the
  default Identity template's sign-in call matches by username, which is not
  always the same value as the e-mail address).
- Newly registered users are automatically assigned the **Reader** role, so
  the `AuthorOrAdmin`-style gate this project *used to* have on article
  authorship is gone entirely — any Reader can publish immediately.
- Registration also collects the new account's `UserProfiles` data (full
  name, bio, country, website, and a profile photo URL) and creates that row
  immediately, so every account has a complete profile from the moment it
  exists (§6.13) rather than only the handful of seeded sample authors.
- The pages share CodeSphere's own layout and styling instead of the
  framework's default, unstyled fallback pages.

One authorization policy remains: `AdminOnly` (required for `/Dashboard`, and
for category/tag management). `/Articles/Manage/*` now only requires the
user to be authenticated — no specific role.

> **Figure 5 — Login page.**
> _Insert screenshot here._ `![Figure 5: Login](images/login.png)`

> **Figure 6 — Register page (account fields and optional profile fields).**
> _Insert screenshot here._ `![Figure 6: Register](images/register.png)`

### 6.11 Administrative Dashboard and Charts

`Pages/Dashboard/Index.cshtml` (Admin-only) shows headline statistics
(article/user/comment/reaction counts) plus three Chart.js visualizations —
a doughnut chart of articles by category, a line chart of articles published
over the last 14 days, and a bar chart of the top five trending articles —
all fed by a JSON page handler (`?handler=StatsJson`) called through the
Fetch API, so the charts can be refreshed without a full page reload. With
the larger seeded dataset, these charts now render meaningfully populated
visualizations rather than one or two data points.

> **Figure 7 — Admin dashboard with charts.**
> _Insert screenshot here._ `![Figure 7: Dashboard](images/dashboard.png)`

### 6.12 AJAX / Fetch API Interactions

Reactions, bookmarking, following, and inline commenting all use the Fetch
API (`wwwroot/js/site.js`, plus a small inline script on the profile page)
against small Razor Pages handlers (`OnPostReactionAsync`,
`OnPostBookmarkAsync`, `OnPostCommentAsync`, `OnPostFollowAsync`) rather than
full-page form posts, updating the DOM in place. Anti-forgery protection is
preserved by configuring `AddAntiforgery(options => options.HeaderName =
"RequestVerificationToken")` and sending the page's anti-forgery token as a
request header from JavaScript.

### 6.13 Public User Profiles and Profile Photos

Every account — self-registered or seeded — has a corresponding
`UserProfiles` row, and every `UserProfiles` column is now actually surfaced
in the UI: `Pages/Users/Index.cshtml` (route `/Users/{id}`) renders the
user's photo (or a placeholder icon if none was supplied), full name,
username, join date, bio, country, website link, follower/following counts
(with an AJAX follow/unfollow button for other users' profiles), and a grid
of their published articles. The same data is available over the API at
`GET /api/users/{id}` (`UsersController`), backed by the shared
`IUserService.GetProfileAsync`. Author names throughout the site (article
cards, article details, comments, the User Activity report) link to this
page.

**Collecting the photo.** Rather than a file upload, a profile photo is
supplied as a **URL** at registration (both the Razor Pages Register form
and the API's `POST /api/auth/register`) and stored directly in
`UserProfiles.ProfileImageURL`, rendered later as `<img src="...">`.

**Security considerations for the photo URL.** Accepting an arbitrary
string from an untrusted client and later rendering it as an image source
has two realistic risks, both addressed by `Core/Common/ImageUrlValidator.cs`
— a single validator shared by the Web Register page and the API's
registration endpoint, so the rule is enforced identically everywhere:

1. **Script-executing pseudo-schemes.** A value like
   `javascript:alert(1)` or a `data:` URI is not a "real" image URL. The
   validator parses the value with `Uri.TryCreate` and only accepts the
   `http` and `https` schemes, rejecting anything else outright before it is
   ever persisted.
2. **Server-Side Request Forgery (SSRF).** The classic risk with
   user-supplied URLs is a server that *fetches* the URL on the attacker's
   behalf, potentially reaching internal-only services. CodeSphere's server
   code never does this — the stored URL is only ever rendered client-side
   in the requesting user's own browser as an `<img src>` tag. Because the
   application performs no server-side HTTP request against the value at
   all, the classic SSRF mitigation (resolving the host and blocking
   private/internal IP ranges) is not applicable here; this is called out
   explicitly in the validator's source comments so the reasoning is not
   lost, rather than adding SSRF-style checks that would be theatre for a
   vulnerability class that cannot occur through this code path.

The validator also rejects values longer than the `UserProfiles.
ProfileImageURL` column (255 characters) with a friendly message, instead of
letting an oversized value reach EF Core and fail as a raw `SqlException`.
The field is optional throughout — an account with no photo shows a Bootstrap
Icons placeholder instead of a broken `<img>` tag.

For the ~230 seeded sample accounts, `DemoDataSeeder` generates a photo URL
for every profile using [ui-avatars.com](https://ui-avatars.com), a public,
`https`-only placeholder-avatar service that renders an initials-based image
from a name — appropriate for demo data, and itself a normal, valid value
under the same `ImageUrlValidator` rule a real user's photo link would have
to satisfy.

> **Figure 14 — Public user profile page.**
> _Insert screenshot here._ `![Figure 14: User profile](images/user-profile.png)`

### 6.14 UI/UX Enhancements and Responsive Design

A second implementation pass focused entirely on usability and responsive
layout, prompted by the platform now holding a realistic amount of data
(225 articles, 220 categories, 228 tags, ~230 users) rather than a handful of
placeholder rows — several UI patterns that looked fine with 5 categories
did not hold up at 220.

**Users directory.** `Pages/Users/Index.cshtml` (route `/Users`) is a new,
paginated, searchable (by username or full name) directory of every
registered user, each linking to their profile page — previously a profile
was only reachable by clicking through from an article or comment. The
individual profile page itself was renamed from `/Users/{id}` to
`/Users/Details/{id}` to make room for the directory at the plain `/Users`
route; every link to a profile across the site (`_Layout.cshtml`, article
cards, article details, comments, the User Activity report) was updated
accordingly.

**Compact pagination.** `Pages/Shared/_Pagination.cshtml` is a single
reusable partial, backed by `Models/PaginationViewModel.cs`, used by every
paginated list (home, My Articles, Bookmarks, Users directory, Categories,
Tags). Rather than rendering a link for every page — which, once the seeded
dataset pushed article listings past 20+ pages, produced a pagination bar
long enough to force horizontal overflow on mobile — it renders a windowed
"1 2 … 8 9 10 … 27 28 29"-style control with Previous/Next buttons, always
showing the first page, the last page, and a small range around the current
page. Each parent page supplies only its own current filter values (as a
`Dictionary<string,string>`, excluding the page number) so the partial can
rebuild `asp-page`/`asp-all-route-data` links that preserve whatever search
was active.

> **Figure 17 — Compact pagination on the home page.**
> _Insert screenshot here._ `![Figure 17: Pagination](images/pagination.png)`

**Home page density.** The home page's page size increased from 8 to 20
articles, a better fit now that there is enough content to paginate
meaningfully.

**Advanced search everywhere articles are listed.** The home page's search
(keyword + column, category, tag, sort) was previously the only place any
of this existed. `IArticleService.SearchByAuthorAsync` and
`IBookmarkService.SearchBookmarksAsync` extend the same capability to "My
Articles" (plus a Draft/Published status filter, since drafts only make
sense to the author) and "Bookmarks" respectively, both now paginated. All
three code paths — the public search, the author-scoped search, and the
bookmark-scoped search — share one implementation of the actual
filter/sort/paging logic, `Core/Common/ArticleFilterHelper.cs`, so the three
"advanced search" experiences cannot drift apart from one another as the
codebase evolves; each service method only supplies a different base
`IQueryable<Article>` (all articles / one author's articles / one user's
bookmarked articles) before handing off to the shared helper.

**Tag picker.** `Pages/Articles/Manage/Create.cshtml` and `Edit.cshtml`
replaced the flat checkbox list of ~228 tags with an intellisense-style
picker (`.tag-picker` in `site.css`, `initTagPicker` in `site.js`): typing
filters an in-memory list of all tags (already available to the page, so no
extra round-trip), selecting one adds it as a removable chip, and a hidden
input per selected tag preserves the existing `Input.TagIds` model binding —
no server-side changes were needed for this, only the input UI.

> **Figure 16 — Tag picker on the Create Article form.**
> _Insert screenshot here._ `![Figure 16: Tag picker](images/tag-picker.png)`

**Categories and Tags search.** Both listing pages gained a keyword search
box (filtering by name) plus the same pagination partial — with 220+
categories and 228+ tags now seeded, an unfiltered, unpaginated list of
either was no longer practical to browse.

**Reports: sticky headers and mobile scrolling.** Both report tables sit
inside a `.report-table-wrapper` div. On desktop, the table's own scrolling
context is the page itself, so `position: sticky; top: var(--navbar-height)`
on the header row keeps the column titles pinned just below the (now also
sticky) navbar as the page scrolls. A media query switches the *same*
wrapper to `overflow-y: auto` with a bounded `max-height` on narrow
viewports, which changes the sticky element's nearest scrolling ancestor to
the wrapper itself — so on mobile, the identical CSS rule instead pins the
header to the top of a small, self-contained scrollbox, and the table can no
longer push the surrounding page out of frame. `table-responsive`-style
horizontal scrolling handles the width dimension the same way it already did
elsewhere. The report page header (title + Print/PDF/Excel/next-report
buttons) switches from a single row to a stacked column
(`flex-column flex-md-row`) below the `md` breakpoint, so the buttons sit
under the title instead of forcing the title to shrink or wrap awkwardly
next to them.

> **Figure 18 — Reports page on mobile.**
> _Insert screenshot here._ `![Figure 18: Mobile reports](images/reports-mobile.png)`

**Toast notifications.** Every `alert()` call in `site.js` (reactions,
bookmarks, comments, following) was replaced with `showToast(message)`,
which builds and shows a Bootstrap 5 toast in the bottom-right corner rather
than blocking the page with a native browser dialog — a small change, but
`alert()` is jarring, blocks all interaction until dismissed, and looks
distinctly out of place next to the rest of the UI.

> **Figure 19 — Toast error notification.**
> _Insert screenshot here._ `![Figure 19: Toast notification](images/toast.png)`

**Sticky navbar, and a smaller footer.** The navbar carries Bootstrap's
`.sticky-top` utility class, staying in view as the page scrolls (its
measured height is also written to a `--navbar-height` CSS variable at
runtime by `site.js`, which the sticky report-table headers, above, offset
against). The footer's text was shortened from "© 2026 - CodeSphere — Web
Programming Final Project" to a centered "© 2026 - CodeSphere" (the year
itself is still computed as `@DateTime.Now.Year`, not hard-coded, so it
won't go stale).

**Dark / light theme.** Uses Bootstrap 5.3's built-in color-mode support
(`data-bs-theme` on `<html>`) rather than a hand-rolled dark palette: a
toggle button in the navbar flips the attribute, persists the choice in
`localStorage`, and a small inline script in `<head>` (before any content
renders) applies the saved preference immediately, avoiding a flash of the
wrong theme on page load. Bootstrap re-themes its own components
automatically; `site.css` adds dark-mode overrides only for the handful of
custom colors this app defines itself (`.article-card`, `.tag-badge`, the
report table header background, `.tag-picker-suggestions`).

> **Figure 20 — Dark theme.**
> _Insert screenshot here._ `![Figure 20: Dark theme](images/dark-theme.png)`

**Long category names and badge overflow.** Categories are now named more
descriptively (e.g. "Database Engineering — Performance Optimization" rather
than "Database"), which exposed a real, easy-to-miss bug: Bootstrap's
`.badge` class sets `white-space: nowrap` by default, so a long category name
inside a badge on a narrow article card would refuse to wrap and force
horizontal overflow. Every badge that renders a category name (home page,
article details, bookmarks, user profiles) now adds `text-wrap text-start`
to opt back into normal wrapping — a one-line fix once identified, but one
that a shorter, hand-picked category list would never have surfaced.

---

## 7. Web API

### 7.1 Design Principles

The API follows REST conventions: resources are addressed by noun-based
routes (`/api/articles`, `/api/categories`, ...), HTTP methods express intent
(`GET` read, `POST` create, `PUT` full update, `DELETE` remove), and HTTP
status codes reflect outcome (`200`/`201`/`204` success, `400` validation
error, `401`/`403` authentication/authorization failure, `404` not found).

### 7.2 Authentication

The API is secured with JWT bearer tokens. `POST /api/auth/register` creates
a new account (with the same optional profile fields, and the same photo-URL
validation, as the Razor Pages Register form) and returns a token
immediately; `POST /api/auth/login` exchanges existing credentials for a
token. Both operate against the same Identity user store used by the web
application, so any seeded or self-registered account works against both
front-ends. Endpoints that don't require a signed-in user are explicitly
marked `[AllowAnonymous]`; everything else requires a valid bearer token, and
Admin-only endpoints additionally require the `AdminOnly` policy.

### 7.3 Endpoint Summary

34 endpoints are exposed across 10 controllers. The full list, with request
and response examples for every endpoint, is documented separately in
**`API_DOCUMENTATION.md`** so it can be read and updated independently of
this report. At a glance:

| Controller | Endpoints | Notes |
|---|---|---|
| Auth | 2 | JWT login and registration |
| Articles | 6 | Search/filter/paginate, get by id, get mine, create, update, delete |
| Categories | 5 | Full CRUD (write operations Admin-only) |
| Tags | 4 | List, get by id, create, delete (write operations Admin-only) |
| Comments | 3 | List by article, add, delete |
| Reactions | 2 | Get breakdown, toggle |
| Bookmarks | 2 | Get mine, toggle |
| Follows | 2 | Get counts, toggle |
| Users | 1 | Public profile (`UserProfiles` data + published articles) |
| Reports | 7 | Trending, user activity, dashboard stats, 4 export endpoints |

Note that the User Activity report endpoints are documented as Anonymous,
matching the corresponding Razor Page's actual (open-to-everyone) access
level, even though the underlying controller action still carries an
`AdminOnly` attribute in code — see the note in `API_DOCUMENTATION.md` §10.

### 7.4 Interactive Documentation (Swagger)

The API is self-documenting through Swashbuckle: every action carries an XML
`<summary>` that Swagger surfaces as endpoint documentation, and a JWT
security scheme is registered so a token obtained from `/api/auth/login` or
`/api/auth/register` can be pasted into Swagger UI's *Authorize* dialog to
test protected endpoints interactively, without a separate client.

> **Figure 12 — Swagger UI listing all documented endpoints.**
> _Insert screenshot here._ `![Figure 12: Swagger UI](images/swagger-endpoints.png)`

> **Figure 13 — Swagger UI JWT authorization dialog.**
> _Insert screenshot here._ `![Figure 13: Swagger Authorize](images/swagger-authorize.png)`

---

## 8. Testing and Quality Assurance

Testing was carried out manually against each functional requirement in
§2.2, covering:

- **Happy paths** — registering (with and without optional profile fields),
  logging in, publishing an article, tagging it, commenting, reacting,
  bookmarking, following another user, viewing a public profile, and
  viewing both reports in every export format.
- **Authorization boundaries** — confirming any authenticated user (not just
  a special role) can reach `/Articles/Manage`, a user cannot edit another
  user's article, and only an Admin can reach `/Dashboard` or manage
  categories/tags.
- **Validation boundaries** — submitting empty/too-short/too-long field
  values and confirming both the client-side and server-side messages
  appear; specifically for the profile photo field, submitting a
  `javascript:` URL, a `data:` URL, and an over-length URL, and confirming
  each is rejected with a clear message rather than being stored.
- **Referential integrity** — confirming a category with existing articles
  cannot be deleted, and that deleting an article cascades to its comments
  and tag associations as intended.
- **Data volume** — confirming the seeded database contains 200+ rows in
  every table, that search/filter/sort behave sensibly across 225 articles
  and 220 categories, and that the reports and dashboard charts render
  correctly at this scale.
- **API contract testing via Swagger UI** — exercising every endpoint listed
  in §7.3 directly through the generated Swagger page, including the
  register → authorize → call protected endpoint flow.

A dedicated automated test project was considered out of scope for this
iteration in favour of broader manual coverage across both front-ends; adding
an xUnit test project against the service layer is listed as future work in
§11.

---

## 9. Error Handling Strategy

Error handling is layered so that failures are caught as close to their
source as possible and never leak an unhandled exception to the user:

1. **Service layer** — expected failures (not found, validation, ownership,
   an invalid profile photo URL) are returned as a
   `ServiceResult`/`ServiceResult<T>` (`Success`, `ErrorMessage`, `Data`), so
   callers can react to a failure without a `try`/`catch`.
2. **Razor Pages** — page handlers check `result.Success` and either
   re-render the form with `ModelState.AddModelError`, or redirect with a
   `TempData["ErrorMessage"]`/`TempData["SuccessMessage"]` banner shown by
   the shared layout.
3. **Custom middleware** — anything that still escapes as a genuine
   exception is caught centrally (§6.7): domain exceptions become friendly
   `/Error/*` pages on the web app or `ProblemDetails` JSON on the API;
   anything unexpected is logged, and — critically — is only hidden from
   the user in non-Development environments, so developers always see the
   real cause locally.
4. **Dedicated error pages** — `/Error`, `/Error/NotFound`,
   `/Error/Forbidden`, and `/Error/BadRequest` present a consistent, on-brand
   message instead of a framework default error page.

---

## 10. Challenges and Solutions

| Challenge | Solution |
|---|---|
| An early design included a separate "Author" role gating who could publish, which turned out to conflict with the platform's actual premise — on a dev.to-style site, every registered user is expected to be able to write. | Removed the `Author` role and the `AuthorOrAdmin` policy entirely; `/Articles/Manage/*` now only requires the caller to be authenticated. Only `Reader` (the automatic default) and `Admin` remain. |
| The `UserProfiles` table existed in the schema and was seeded for a handful of sample authors, but nothing in the application ever read or displayed it — new self-registered accounts didn't even get a row. | Added a public profile page (`/Users/Details/{id}`, and `GET /api/users/{id}`) that renders every `UserProfiles` column plus the user's published articles, plus a searchable directory at `/Users`; extended Register (both front-ends) to collect these fields and always create the row, even when every optional field is left blank. |
| Accepting a user-supplied profile photo link safely, without building actual image upload/storage. | Store a URL rather than a file, but validate it through a single shared `ImageUrlValidator`: absolute `http`/`https` only (blocking `javascript:`/`data:`), length-capped to match the database column. Documented explicitly why classic SSRF mitigations don't apply here — the server never fetches the URL itself, only the browser does, client-side. |
| Demonstrating search, filtering, pagination, and the reports convincingly needs real volume, but hand-writing 200+ genuinely distinct, technically accurate articles is not practically feasible. | Built `DemoDataSeeder`, a programmatic generator that composes each article from independently-templated, topic-parameterized paragraphs (introduction, core concept, practical notes, pitfalls, conclusion) drawn from curated pools, producing genuinely varied, on-topic technical writing at scale rather than Lorem Ipsum or verbatim-duplicated text — see §5.5. |
| Reproducing the original `TrendingArticlesView`/`UserActivityView` SQL views and `GetEngagementScore`/`GetTotalFollowers` scalar functions in a code-first world. | Re-implemented the identical logic as LINQ projections in `ReportService`, so the reporting numbers match the original SQL design exactly while remaining fully covered by EF Core's change-tracking-free (`AsNoTracking`) read path. |
| The framework's default Identity Register page failed on every request, and the default Login page could not authenticate seeded accounts (a missing `IEmailSender` registration, and a username/email sign-in mismatch, respectively — diagnosed in an earlier iteration of this project). | Replaced the framework's default Identity UI with custom Login/Register pages that don't depend on `IEmailSender`, and that resolve the account by e-mail (`FindByEmailAsync`) before signing in, rather than passing the typed e-mail through as if it were the username. |
| Keeping identical business rules and validation available to both a server-rendered UI and a JSON API without duplicating logic — now including registration and profile creation. | All business logic lives once, in `CodeSphere.Core` services behind interfaces (`IUserService` included); both `CodeSphere.Web` and `CodeSphere.Api` are thin callers that translate `ServiceResult`s into their respective UI or HTTP responses, and both registration flows call the same `ImageUrlValidator`. |
| At the original seeded scale, a full-width pagination bar listing every page number was harmless; once the demo dataset made article listings run to dozens of pages, that same bar became long enough to force the whole page into horizontal scroll on mobile. | Replaced it with a single, shared, windowed pagination partial (§6.14) used by every paginated list, rendering at most ~7 page controls regardless of how many pages exist. |
| Three different pages (home search, "My Articles", "Bookmarks") needed the same keyword/category/tag/sort filtering logic applied to three different base sets of articles (all published, one author's, one user's bookmarks) — a natural place for the logic to quietly diverge across copy-pasted implementations. | Extracted the filter/sort/paging logic once into `ArticleFilterHelper`, called by all three service methods; each supplies only its own base `IQueryable<Article>`. |
| Longer, more descriptive category names (an intentional change to reach the 200-row target meaningfully, §5.5) broke an existing, previously-invisible assumption: Bootstrap's `.badge` component doesn't wrap text by default, so a long name inside a badge silently forced horizontal overflow on narrow article cards. | Added `text-wrap text-start` to every badge that renders a category name; caught by testing the UI against the actual seeded data volume rather than only the original handful of short category names. |
| Sticky report table headers needed to behave differently in two contexts: docking below the (also sticky) navbar as the *whole page* scrolls on desktop, but staying within a small, self-contained box on mobile so the table itself doesn't push the page out of frame. | One `position: sticky` rule handles both: its behavior depends entirely on which element is its nearest *scrolling* ancestor. A media query toggles `overflow-y: auto` + `max-height` on the wrapper only below the mobile breakpoint, which changes that scrolling ancestor from the page to the wrapper itself — no JavaScript required. |

---

## 11. Future Enhancements

- A dedicated "edit profile" page — profile fields are currently only
  collected once, at registration; there's no self-serve way to update them
  afterward.
- Real image hosting/upload for profile photos, as an alternative to
  supplying a URL, for users who don't already have one to link to.
- A dedicated automated test project (xUnit) covering the service layer in
  isolation with an in-memory or SQLite EF Core provider.
- Real transactional e-mail (replacing the no-op `IEmailSender`) for password
  reset and e-mail confirmation flows.
- External OAuth login providers (GitHub, Google) — a natural fit for a
  developer-focused platform.
- Rich-text/Markdown rendering for article content.
- Caching of the Trending Articles ranking and dashboard statistics to
  reduce repeated aggregate queries under load, now more relevant with a
  225-article, 230-user dataset.
- Expose `SearchByAuthorAsync`, `SearchBookmarksAsync`, and `SearchUsersAsync`
  through `CodeSphere.Api` as well — they currently only back the Razor
  Pages UI (My Articles, Bookmarks, Users directory); API clients still only
  have the unfiltered `GET /api/articles/mine` and `GET /api/bookmarks`.
- Debounced, live "search as you type" for the Categories/Tags/tag-picker
  search boxes, rather than requiring a form submit — feasible client-side
  since the full list is already on the page, but left as a page-reload
  search for this iteration to keep behavior consistent and predictable
  across all the search boxes in the app.

---

## 12. Conclusion

CodeSphere demonstrates a complete, production-shaped ASP.NET Core solution
built around a single shared data and business-logic layer, consumed by both
a server-rendered Razor Pages application and a documented REST API. Beyond
satisfying the course's mandatory checklist — schema design, EF Core CRUD,
Razor Pages CRUD, validation, a documented API, dependency injection, custom
middleware, search/filtering, print-ready reporting, and centralized error
handling — the project layers in authentication, public user profiles with a
securely-validated photo URL, pagination, an administrative analytics
dashboard, PDF/Excel export, and an AJAX-driven interaction model, all
exercised meaningfully by a 200+-row-per-table seeded dataset rather than a
handful of placeholder records. Revisiting and removing the artificial
"Author" role, and making the previously-unused `UserProfiles` table an
actual, visible feature, were both useful reminders that a schema faithfully
implemented is not the same thing as a schema faithfully *used*.

---

## 13. References

1. Microsoft. *ASP.NET Core Documentation.* https://learn.microsoft.com/aspnet/core/
2. Microsoft. *Entity Framework Core Documentation.* https://learn.microsoft.com/ef/core/
3. Microsoft. *ASP.NET Core Identity.* https://learn.microsoft.com/aspnet/core/security/authentication/identity
4. Microsoft. *Introduction to Razor Pages.* https://learn.microsoft.com/aspnet/core/razor-pages/
5. Microsoft. *JSON Web Token (JWT) Authentication.* https://learn.microsoft.com/aspnet/core/security/authentication/
6. Swashbuckle.AspNetCore. *Swagger / OpenAPI tooling for ASP.NET Core.* https://github.com/domaindrivendev/Swashbuckle.AspNetCore
7. QuestPDF. *Documentation.* https://www.questpdf.com/
8. ClosedXML. *Documentation.* https://github.com/ClosedXML/ClosedXML
9. Chart.js. *Documentation.* https://www.chartjs.org/docs/latest/
10. IETF. *RFC 7807 — Problem Details for HTTP APIs.* https://www.rfc-editor.org/rfc/rfc7807
11. OWASP. *Server-Side Request Forgery Prevention Cheat Sheet.* https://cheatsheetseries.owasp.org/cheatsheets/Server_Side_Request_Forgery_Prevention_Cheat_Sheet.html
12. Fowler, M. *Patterns of Enterprise Application Architecture.* Addison-Wesley, 2002. (Service layer / DTO patterns.)
