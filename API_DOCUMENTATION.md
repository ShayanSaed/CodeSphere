# CodeSphere REST API Documentation

**Base URL (Development):** `https://localhost:7101/api`
**Interactive docs (Swagger UI):** `https://localhost:7101/swagger`
**Content type:** `application/json` for all requests and responses (file
export endpoints return binary content with the appropriate `Content-Type`).

This document lists every one of the 34 endpoints exposed by `CodeSphere.Api`
across 10 controllers, grouped by controller, with authentication
requirements, parameters, and example request/response payloads for every
single endpoint — not just a representative few. It mirrors — and can be
read alongside — the interactive Swagger UI generated at runtime from the
same controllers.

---

## Contents

1. [Authentication](#1-authentication)
2. [Articles](#2-articles)
3. [Categories](#3-categories)
4. [Tags](#4-tags)
5. [Comments](#5-comments)
6. [Reactions](#6-reactions)
7. [Bookmarks](#7-bookmarks)
8. [Follows](#8-follows)
9. [Users](#9-users)
10. [Reports](#10-reports)
11. [Common Response Shapes](#11-common-response-shapes)
12. [HTTP Status Code Conventions](#12-http-status-code-conventions)

---

## 1. Authentication

The API uses **JWT Bearer authentication**. Register or log in to obtain a
token, then send it on every subsequent request as:

```
Authorization: Bearer <access_token>
```

Endpoints marked **Anonymous** below require no token. Endpoints marked
**Authenticated** require any valid token. Endpoints marked **Admin only**
additionally require the token's user to be in the `Admin` role.

There is no separate "Author" role anywhere in the API — every registered
account (role `Reader`, or `Admin`) may create, edit, and delete their own
articles. `Reader` is the only role a self-registered account can end up
with; `Admin` is only ever assigned by seeding or directly in the database.

### `POST /api/auth/register`

Create a new account and immediately receive a JWT access token for it —
equivalent to what the Razor Pages Register form does, for API-only clients.

- **Auth:** Anonymous
- **Request body:**

```json
{
  "userName": "new_dev",
  "email": "new_dev@example.com",
  "password": "StrongPass1",
  "fullName": "Jordan Lee",
  "bio": "Frontend engineer exploring backend development.",
  "country": "Canada",
  "websiteURL": "https://jordanlee.dev",
  "profileImageURL": "https://example.com/photos/jordan.jpg"
}
```

Only `userName`, `email`, and `password` are required; the remaining fields
populate the new account's `UserProfiles` row (shown on its profile page,
§9) and may be omitted.

> **Security note:** `profileImageURL` is validated server-side before
> anything is saved — it must be an absolute `http://` or `https://` URL
> (schemes such as `javascript:` or `data:` are rejected outright) and no
> longer than 255 characters. The server never fetches this URL itself; it
> is only ever rendered client-side as an `<img src="...">`, so there is no
> server-side SSRF risk from this field — the validation exists purely to
> stop a malicious value from being persisted in the first place.

- **Success response — `201 Created`** (with a `Location` header pointing at
  `GET /api/users/{id}` for the new account):

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-07-12T20:00:00Z",
  "userId": 231,
  "username": "new_dev",
  "roles": ["Reader"]
}
```

- **Error response — `400 Bad Request`** (Data Annotation validation, e.g. a
  short password or an invalid photo URL):

```json
{
  "errors": {
    "Password": ["Password must be at least 8 characters long."],
    "ProfileImageURL": ["Profile image URL must start with http:// or https://."]
  },
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

- **Error response — `400 Bad Request`** (username or email already taken,
  reported by ASP.NET Core Identity):

```json
{ "message": "Username 'new_dev' is already taken." }
```

### `POST /api/auth/login`

Exchange an email/password pair for a JWT access token.

- **Auth:** Anonymous
- **Request body:**

```json
{
  "email": "admin@codesphere.dev",
  "password": "Admin@12345"
}
```

- **Success response — `200 OK`:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-07-08T18:00:00Z",
  "userId": 1,
  "username": "admin",
  "roles": ["Admin"]
}
```

- **Error response — `401 Unauthorized`:**

```json
{ "message": "Invalid email or password." }
```

---

## 2. Articles

Base route: `/api/articles`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/articles` | Anonymous | Search, filter, sort, and paginate published articles |
| GET | `/api/articles/{id}` | Anonymous | Full article detail (increments the view count) |
| GET | `/api/articles/mine` | Authenticated | The current user's own articles, including drafts |
| POST | `/api/articles` | Authenticated | Create a new article as the current user |
| PUT | `/api/articles/{id}` | Authenticated | Update an article you own (or any article, if Admin) |
| DELETE | `/api/articles/{id}` | Authenticated | Delete an article you own (or any article, if Admin) |

### `GET /api/articles`

Query parameters (all optional):

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `keyword` | string | — | Free-text search term |
| `searchColumn` | string | `Title` | `Title` or `Content` |
| `categoryId` | int | — | Filter by category |
| `tagId` | int | — | Filter by tag |
| `status` | string | — | `Draft` or `Published` |
| `sortBy` | string | `Newest` | `Newest`, `Trending`, or `MostViewed` |
| `pageNumber` | int | `1` | 1-based page index |
| `pageSize` | int | `10` | Capped at 50 server-side |

Example request:

```
GET /api/articles?keyword=sql&categoryId=2&sortBy=Trending&pageNumber=1&pageSize=10
```

Example response — `200 OK`:

```json
{
  "items": [
    {
      "articleID": 1,
      "title": "SQL Optimization Tips",
      "author": "reza_sql",
      "authorId": 12,
      "categoryName": "Database Engineering — Performance Optimization",
      "publishDate": "2026-06-08T00:00:00Z",
      "viewCount": 123,
      "readingTime": 5,
      "status": "Published",
      "commentCount": 2,
      "reactionCount": 2,
      "engagementScore": 133,
      "tags": ["SQL"]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### `GET /api/articles/{id}`

Example response — `200 OK`:

```json
{
  "articleID": 1,
  "title": "SQL Optimization Tips",
  "author": "reza_sql",
  "authorId": 12,
  "categoryName": "Database Engineering — Performance Optimization",
  "categoryId": 2,
  "publishDate": "2026-06-08T00:00:00Z",
  "viewCount": 124,
  "readingTime": 5,
  "status": "Published",
  "commentCount": 2,
  "reactionCount": 2,
  "engagementScore": 134,
  "tags": ["SQL"],
  "content": "Content about SQL performance and index tuning...",
  "comments": [
    {
      "commentID": 2,
      "articleID": 1,
      "author": "sara_js",
      "userID": 11,
      "commentText": "Great SQL tips!",
      "commentDate": "2026-06-09T00:00:00Z"
    }
  ],
  "reactionBreakdown": { "Like": 1, "Love": 1 }
}
```

Response — `404 Not Found` if the article does not exist (empty body).

### `GET /api/articles/mine`

- **Auth:** Authenticated

Example response — `200 OK` (same item shape as the list inside
`GET /api/articles`, but unfiltered by status, so drafts are included):

```json
[
  {
    "articleID": 11,
    "title": "AI Roadmap",
    "author": "john_doe",
    "authorId": 14,
    "categoryName": "Artificial Intelligence — Case Studies",
    "publishDate": null,
    "viewCount": 0,
    "readingTime": 10,
    "status": "Draft",
    "commentCount": 0,
    "reactionCount": 0,
    "engagementScore": 0,
    "tags": ["AI"]
  }
]
```

### `POST /api/articles`

- **Auth:** Authenticated
- **Request body:**

```json
{
  "categoryID": 2,
  "title": "Understanding SQL Server Indexes",
  "content": "A deep dive into clustered vs non-clustered indexes and when to use each one...",
  "readingTime": 7,
  "status": "Published",
  "tagIds": [1, 7]
}
```

- **Success response — `201 Created`** (with a `Location` header pointing at
  `GET /api/articles/{id}`), body is the created article's detail DTO (same
  shape as `GET /api/articles/{id}`).
- **Error response — `400 Bad Request`:**

```json
{
  "errors": {
    "Title": ["Title is required."],
    "Content": ["Content should be at least 20 characters."]
  },
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

### `PUT /api/articles/{id}`

- **Auth:** Authenticated (owner or Admin)
- **Request body:** same shape as create, plus `articleID` matching the route id:

```json
{
  "articleID": 12,
  "categoryID": 2,
  "title": "SQL Server Indexing (Updated)",
  "content": "A deep dive into clustered vs non-clustered indexes and when to use each one, now with a benchmark section...",
  "readingTime": 9,
  "status": "Published",
  "tagIds": [1]
}
```

- **Success response:** `204 No Content`
- **Error response — `400 Bad Request`** (validation or unknown category):

```json
{ "message": "The specified category does not exist." }
```

- **Error response — `403 Forbidden`** — returned (empty body) if the
  authenticated user neither owns the article nor is an Admin.

### `DELETE /api/articles/{id}`

- **Auth:** Authenticated (owner or Admin)
- **Success response:** `204 No Content`
- **Error response — `404 Not Found`** — no such article (empty body).
- **Error response — `403 Forbidden`** — the authenticated user neither owns
  the article nor is an Admin (empty body).

---

## 3. Categories

Base route: `/api/categories`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/categories` | Anonymous | List all categories |
| GET | `/api/categories/{id}` | Anonymous | Get a single category |
| POST | `/api/categories` | Admin only | Create a category |
| PUT | `/api/categories/{id}` | Admin only | Update a category |
| DELETE | `/api/categories/{id}` | Admin only | Delete a category (fails if it still has articles) |

### `GET /api/categories`

Example response — `200 OK`:

```json
[
  { "categoryID": 2, "categoryName": "Database Engineering — Fundamentals", "description": "Articles about fundamentals in database engineering.", "articleCount": 12 },
  { "categoryID": 3, "categoryName": "Artificial Intelligence — Case Studies", "description": "Articles about case studies in artificial intelligence.", "articleCount": 5 }
]
```

### `GET /api/categories/{id}`

Example response — `200 OK`:

```json
{ "categoryID": 2, "categoryName": "Database Engineering — Fundamentals", "description": "Articles about fundamentals in database engineering.", "articleCount": 12 }
```

Response — `404 Not Found` if the category does not exist (empty body).

### `POST /api/categories`

- **Auth:** Admin only
- **Request body:**

```json
{ "categoryName": "Cloud Computing — Cost Optimization", "description": "Reducing cloud spend without sacrificing reliability." }
```

- **Success response — `201 Created`** (with a `Location` header pointing at
  `GET /api/categories/{id}`), body is the created `CategoryDto`.
- **Error response — `400 Bad Request`:**

```json
{ "message": "A category with this name already exists." }
```

### `PUT /api/categories/{id}`

- **Auth:** Admin only
- **Request body:**

```json
{ "categoryID": 2, "categoryName": "Database Engineering — Core Concepts", "description": "Updated description." }
```

- **Success response:** `204 No Content`
- **Error response — `400 Bad Request`:**

```json
{ "message": "A category with this name already exists." }
```

### `DELETE /api/categories/{id}`

- **Auth:** Admin only
- **Success response:** `204 No Content`
- **Error response — `400 Bad Request`** (category still has articles assigned to it):

```json
{ "message": "Cannot delete a category that still has articles assigned to it." }
```

---

## 4. Tags

Base route: `/api/tags`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/tags` | Anonymous | List all tags |
| GET | `/api/tags/{id}` | Anonymous | Get a single tag |
| POST | `/api/tags` | Admin only | Create a tag |
| DELETE | `/api/tags/{id}` | Admin only | Delete a tag |

### `GET /api/tags`

Example response — `200 OK`:

```json
[
  { "tagID": 1, "tagName": "CSharp", "description": "CSharp-related content.", "articleCount": 9 },
  { "tagID": 62, "tagName": "Docker", "description": "Docker-related content.", "articleCount": 14 }
]
```

### `GET /api/tags/{id}`

Example response — `200 OK`:

```json
{ "tagID": 1, "tagName": "CSharp", "description": "CSharp-related content.", "articleCount": 9 }
```

Response — `404 Not Found` if the tag does not exist (empty body).

### `POST /api/tags`

- **Auth:** Admin only
- **Request body:**

```json
{ "tagName": "WebAssembly", "description": "Running near-native code in the browser." }
```

- **Success response — `201 Created`** (with a `Location` header pointing at
  `GET /api/tags/{id}`), body is the created `TagDto`.
- **Error response — `400 Bad Request`:**

```json
{ "message": "A tag with this name already exists." }
```

### `DELETE /api/tags/{id}`

- **Auth:** Admin only
- **Success response:** `204 No Content`
- **Error response — `400 Bad Request`** (no such tag):

```json
{ "message": "Tag not found." }
```

---

## 5. Comments

Base route: `/api/articles/{articleId}/comments`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/articles/{articleId}/comments` | Anonymous | List comments on an article |
| POST | `/api/articles/{articleId}/comments` | Authenticated | Add a comment as the current user |
| DELETE | `/api/articles/{articleId}/comments/{commentId}` | Authenticated | Delete your own comment (or any, if Admin) |

### `GET /api/articles/{articleId}/comments`

Example response — `200 OK`:

```json
[
  {
    "commentID": 3,
    "articleID": 1,
    "author": "parsa_backend",
    "userID": 13,
    "commentText": "Excellent article. Thanks for sharing!",
    "commentDate": "2026-07-02T16:15:46Z"
  },
  {
    "commentID": 2,
    "articleID": 1,
    "author": "sara_js",
    "userID": 11,
    "commentText": "Great SQL tips!",
    "commentDate": "2026-06-09T00:00:00Z"
  }
]
```

### `POST /api/articles/{articleId}/comments`

- **Auth:** Authenticated
- **Request body:**

```json
{ "commentText": "This helped me a lot, thanks!" }
```

- **Success response — `200 OK`:**

```json
{
  "commentID": 15,
  "articleID": 1,
  "author": "john_doe",
  "userID": 14,
  "commentText": "This helped me a lot, thanks!",
  "commentDate": "2026-07-08T12:00:00Z"
}
```

- **Error response — `400 Bad Request`:**

```json
{ "message": "Comment cannot be empty." }
```

### `DELETE /api/articles/{articleId}/comments/{commentId}`

- **Auth:** Authenticated (comment owner or Admin)
- **Success response:** `204 No Content`
- **Error response — `400 Bad Request`:**

```json
{ "message": "You are not allowed to delete this comment." }
```

---

## 6. Reactions

Base route: `/api/articles/{articleId}/reactions`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/articles/{articleId}/reactions` | Anonymous | Get the reaction-type breakdown for an article |
| POST | `/api/articles/{articleId}/reactions?type=Like` | Authenticated | Toggle a reaction (`Like`, `Love`, or `Idea`) for the current user |

> The Razor Pages UI shows these three reaction types as icons rather than
> text (thumbs-up for Like, heart for Love, lightbulb for Idea) — the API
> itself still identifies them by these three string values.

### `GET /api/articles/{articleId}/reactions`

Example response — `200 OK`:

```json
{ "Like": 1, "Love": 1 }
```

A reaction type with zero reactions is simply absent from the object (there
is no `"Idea": 0` entry unless at least one user reacted with `Idea`).

### `POST /api/articles/{articleId}/reactions?type=Love`

- **Auth:** Authenticated
- **Request body:** none (the reaction type is a query parameter)
- **Success response — `200 OK`** (the full breakdown after the toggle):

```json
{ "Like": 1, "Love": 2 }
```

- **Error response — `400 Bad Request`** (invalid type):

```json
{ "message": "Invalid reaction type." }
```

- **Error response — `400 Bad Request`** (no such article):

```json
{ "message": "Article not found." }
```

---

## 7. Bookmarks

Base route: `/api/bookmarks` (every endpoint requires authentication)

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/bookmarks` | Authenticated | The current user's bookmarked articles |
| POST | `/api/bookmarks/{articleId}` | Authenticated | Toggle a bookmark on/off |

### `GET /api/bookmarks`

Example response — `200 OK` (same item shape as an article list — see §2):

```json
[
  {
    "articleID": 3,
    "title": "Database Normalization",
    "author": "reza_sql",
    "authorId": 12,
    "categoryName": "Database Engineering — Fundamentals",
    "publishDate": "2026-06-03T00:00:00Z",
    "viewCount": 180,
    "readingTime": 6,
    "status": "Published",
    "commentCount": 1,
    "reactionCount": 0,
    "engagementScore": 183,
    "tags": ["DatabaseDesign"]
  }
]
```

### `POST /api/bookmarks/{articleId}`

- **Auth:** Authenticated
- **Request body:** none
- **Success response — `200 OK`:**

```json
true
```

(`true` = now bookmarked, `false` = now unbookmarked — this endpoint always
toggles.)

- **Error response — `400 Bad Request`** (no such article):

```json
{ "message": "Article not found." }
```

---

## 8. Follows

Base route: `/api/follows`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/follows/{userId}/counts` | Anonymous | Follower/following counts for a user |
| POST | `/api/follows/{userId}` | Authenticated | Toggle following a user |

### `GET /api/follows/{userId}/counts`

Example response — `200 OK`:

```json
{ "followers": 3, "following": 1 }
```

### `POST /api/follows/{userId}`

- **Auth:** Authenticated
- **Request body:** none
- **Success response — `200 OK`:**

```json
true
```

(`true` = now following, `false` = now unfollowed — this endpoint always
toggles.)

- **Error response — `400 Bad Request`** (trying to follow yourself):

```json
{ "message": "You cannot follow yourself." }
```

- **Error response — `400 Bad Request`** (no such user):

```json
{ "message": "User not found." }
```

---

## 9. Users

Base route: `/api/users`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/users/{id}` | Anonymous | A user's public profile: `UserProfiles` data plus their published articles |

### `GET /api/users/{id}`

Backs the public profile page at `/Users/{id}` in the Razor Pages app —
every field from the `UserProfiles` table, plus basic account info, follower
counts, and the list of the user's published articles.

Example response — `200 OK`:

```json
{
  "userId": 12,
  "username": "reza_sql",
  "joinDate": "2025-03-05T00:00:00Z",
  "fullName": "Reza Hosseini",
  "bio": "Database engineer and SQL optimization enthusiast.",
  "country": "Iran",
  "websiteURL": "https://reza.dev",
  "profileImageURL": "https://ui-avatars.com/api/?name=Reza+Hosseini&background=random&size=256",
  "followerCount": 3,
  "followingCount": 1,
  "articles": [
    {
      "articleID": 1,
      "title": "SQL Optimization Tips",
      "author": "reza_sql",
      "authorId": 12,
      "categoryName": "Database Engineering — Fundamentals",
      "publishDate": "2026-06-08T00:00:00Z",
      "viewCount": 123,
      "readingTime": 5,
      "status": "Published",
      "commentCount": 2,
      "reactionCount": 2,
      "engagementScore": 133,
      "tags": ["SQL"]
    }
  ]
}
```

`fullName`, `bio`, `country`, `websiteURL`, and `profileImageURL` are all
nullable — a user who left every optional field blank at registration (see
§1) still gets a full response, just with those fields set to `null` and an
empty `articles` array if they haven't published anything yet.

Response — `404 Not Found` if the user does not exist (empty body).

---

## 10. Reports

Base route: `/api/reports`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/reports/trending?top=20` | Anonymous | Trending articles ranked by engagement score |
| GET | `/api/reports/user-activity` | Anonymous | Articles/comments/reactions/followers per user |
| GET | `/api/reports/dashboard` | Admin only | Aggregate stats used by the admin dashboard charts |
| GET | `/api/reports/trending/export/pdf` | Anonymous | Trending report as a PDF file |
| GET | `/api/reports/trending/export/excel` | Anonymous | Trending report as an Excel file |
| GET | `/api/reports/user-activity/export/pdf` | Anonymous | User Activity report as a PDF file |
| GET | `/api/reports/user-activity/export/excel` | Anonymous | User Activity report as an Excel file |

The User Activity report and its exports are Anonymous because the
corresponding Razor Page, `/Reports/UserActivity`, is open to every visitor —
matching the rest of this table to that page's actual access level.

### `GET /api/reports/trending?top=5`

Example response — `200 OK`:

```json
[
  {
    "articleID": 10,
    "title": "Fullstack Development Guide",
    "author": "tina_fullstack",
    "categoryName": "Web Development — Case Studies",
    "viewCount": 500,
    "commentCount": 1,
    "reactionCount": 1,
    "engagementScore": 505
  }
]
```

### `GET /api/reports/user-activity`

Example response — `200 OK`:

```json
[
  {
    "userID": 12,
    "username": "reza_sql",
    "fullName": "Reza Hosseini",
    "totalArticles": 2,
    "totalComments": 0,
    "totalReactions": 0,
    "totalFollowers": 3
  }
]
```

### `GET /api/reports/dashboard`

- **Auth:** Admin only

Example response — `200 OK`:

```json
{
  "totalArticles": 225,
  "publishedArticles": 207,
  "draftArticles": 18,
  "totalUsers": 231,
  "totalComments": 260,
  "totalReactions": 260,
  "articlesByCategory": [
    { "categoryName": "Web Development — Fundamentals", "count": 3 },
    { "categoryName": "Database Engineering — Performance Optimization", "count": 2 }
  ],
  "articlesOverTime": [
    { "date": "2026-06-29T00:00:00Z", "count": 1 },
    { "date": "2026-06-30T00:00:00Z", "count": 0 }
  ],
  "topArticles": [
    {
      "articleID": 10,
      "title": "Fullstack Development Guide",
      "author": "tina_fullstack",
      "categoryName": "Web Development — Case Studies",
      "viewCount": 500,
      "commentCount": 1,
      "reactionCount": 1,
      "engagementScore": 505
    }
  ]
}
```

- **Error response — `403 Forbidden`** — returned (empty body) if the caller
  is authenticated but not an Admin.

### Export endpoints

`trending/export/pdf`, `trending/export/excel`, `user-activity/export/pdf`,
and `user-activity/export/excel` all return binary file content directly
rather than JSON:

| Endpoint | Content-Type | Filename |
|---|---|---|
| `trending/export/pdf` | `application/pdf` | `TrendingArticles.pdf` |
| `trending/export/excel` | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` | `TrendingArticles.xlsx` |
| `user-activity/export/pdf` | `application/pdf` | `UserActivity.pdf` |
| `user-activity/export/excel` | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` | `UserActivity.xlsx` |

---

## 11. Common Response Shapes

### Paged list (`PagedResult<T>`)

Used by `GET /api/articles`.

```json
{
  "items": [ /* ... */ ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 225,
  "totalPages": 23,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Validation error (`ValidationProblemDetails`, `400 Bad Request`)

Returned whenever a request body fails Data Annotation validation.

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

### Simple message error

Returned by service-level failures that aren't Data Annotation validation
errors (e.g. business rules).

```json
{ "message": "A category with this name already exists." }
```

---

## 12. HTTP Status Code Conventions

| Status | Meaning in this API |
|---|---|
| `200 OK` | Successful `GET`, or a successful `POST` that returns data (e.g. reactions, bookmarks, follows, login) |
| `201 Created` | A resource was created (`POST /api/auth/register`, `POST /api/articles`, `POST /api/categories`, `POST /api/tags`); the `Location` header points at the new resource |
| `204 No Content` | A successful `PUT` or `DELETE` with no body to return |
| `400 Bad Request` | Data Annotation validation failure, or a business-rule violation reported by the service layer |
| `401 Unauthorized` | Missing/invalid JWT, or invalid login credentials |
| `403 Forbidden` | Valid token, but the user lacks the required role or resource ownership |
| `404 Not Found` | The requested resource does not exist |
| `500 Internal Server Error` | An unexpected exception; returned as an RFC 7807 `ProblemDetails` JSON body by `ApiExceptionHandlingMiddleware`, never a raw stack trace |
