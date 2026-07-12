# Bookstore Management System

  A .NET 10 Web API for managing books, authors, genres, and reviews, with JWT
  authentication, role-based authorization, an hourly scheduled import job, and
  OpenAPI/Swagger documentation.

  ## Tech Stack

  - **.NET 10** Web API (controllers)
  - **EF Core 10** + **SQL Server LocalDB** (code-first, migrations)
  - **Dapper** for the raw-SQL top-10 endpoint
  - **JWT bearer** auth with role-based policies
  - **Quartz.NET** for the hourly import job
  - **Serilog** for structured logging
  - **Swagger UI** (`/swagger`) + **Scalar** (`/scalar/v1`) for docs
  - **xUnit**, **Testcontainers**, **NSubstitute**, **FluentAssertions** for tests

  ## Project Structure

  | Project | Responsibility |
  |---------|----------------|
  | `Bookstore.Data` | Entities, `DbContext`, EF configurations, migrations, Dapper reads, mock import API |
  | `Bookstore.Application` | Services, DTOs, custom validators, import logic |
  | `Bookstore.PublicAPI` | Controllers, auth, middleware, Quartz job, composition root |
  | `Bookstore.Tests` | Unit + integration tests |

  Dependency direction: `Data ← Application ← PublicAPI`, with `Tests` referencing all.

  ## Prerequisites

  - [.NET 10 SDK](https://dotnet.microsoft.com/download)
  - **SQL Server LocalDB** (installed with the Visual Studio "ASP.NET and web development" workload)
  - **Docker Desktop** — only required to run the integration tests (Testcontainers)

  ## Getting Started

  1. Clone the repo:
     ```bash
     git clone <your-repo-url>
     cd Bookstore
  2. The default connection string (in Bookstore.PublicAPI/appsettings.json) points at LocalDB:
  Server=(localdb)\MSSQLLocalDB;Database=BookstoreDb;Trusted_Connection=True;TrustServerCertificate=True
  2. Adjust if your SQL instance differs.
  3. Run the API:
  dotnet run --project Bookstore.PublicAPI
  3. On startup the app automatically applies migrations and seeds sample data — no manual DB setup needed.
  4. Open the docs in your browser:
    - Swagger UI: https://localhost:<port>/swagger
    - Scalar: https://localhost:<port>/scalar/v1

  Authentication

  All book endpoints require a JWT. Get one from the login endpoint.

  Demo users:

  ┌──────────┬────────────┬───────────┬────────────────────┐
  │ Username │  Password  │   Role    │       Access       │
  ├──────────┼────────────┼───────────┼────────────────────┤
  │ reader   │ Reader123! │ Read      │ GET endpoints only │
  ├──────────┼────────────┼───────────┼────────────────────┤
  │ writer   │ Writer123! │ ReadWrite │ All endpoints      │
  └──────────┴────────────┴───────────┴────────────────────┘

  Flow:
  1. POST /api/auth/login with { "username": "writer", "password": "Writer123!" } → returns a token.
  2. In Swagger, click Authorize, paste the token, and all requests include it.
  3. Or send the header manually: Authorization: Bearer <token>.

  ▎ The demo credentials are hardcoded for evaluation only.

  Endpoints

  ┌────────┬───────────────────────┬──────────────────┬──────────────────────────────────────────────────────────┐
  │ Method │         Route         │       Role       │                          Notes                           │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ POST   │ /api/auth/login       │ anonymous        │ Returns a JWT                                            │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ GET    │ /api/books            │ Read / ReadWrite │ All books (Title, Authors, Genres, Avg Rating) — EF Core │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ GET    │ /api/books/top10      │ Read / ReadWrite │ Top 10 by average rating — raw SQL / Dapper              │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ POST   │ /api/books            │ ReadWrite        │ Create a book                                            │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ PUT    │ /api/books/{id}/price │ ReadWrite        │ Update price only                                        │
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ DELETE │ /api/books/{id}       │ ReadWrite        │ Delete a book (reviews cascade; authors/genres kept)     |    
  │        |                       |                  | NOTE: it would be better if it was soft-deletion         |     
  ├────────┼───────────────────────┼──────────────────┼──────────────────────────────────────────────────────────┤
  │ POST   │ /api/import           │ ReadWrite        │ Manually trigger the import (also runs hourly)           │
  └────────┴───────────────────────┴──────────────────┴──────────────────────────────────────────────────────────┘

  Scheduled Import

  - Runs every hour via Quartz.NET (0 0 * * * ?).
  - Simulates a third-party API returning 100,000 books (mocked, no real calls).
  - Matches by title (trimmed, case-insensitive) and skips books already present.
  - Existing titles are loaded once into a HashSet for O(1) matching — no per-book DB queries.
  - Inserts in batches; authors/genres are de-duplicated by name.
  - [DisallowConcurrentExecution] prevents overlapping runs.

  Typo handling (bonus): exact matching won't catch typos like "Crime" vs "Criem".
  TitleMatcher includes a Levenshtein implementation for fuzzy matching. At 100k scale,
  naive fuzzy matching is O(N×M), so production would use DB-side trigram similarity
  (PostgreSQL pg_trgm / SQL Server full-text), a phonetic key (Soundex/Metaphone), or a
  search index (Elasticsearch), combined with "blocking" to reduce comparisons. See comments
  in BookImportService.

  Running Tests

  dotnet test
  - Unit tests run with no external dependencies (EF InMemory).
  - Integration tests use Testcontainers and require Docker Desktop running.
  They spin up a throwaway SQL Server, exercise real endpoints, and verify the
  authorization boundary (401 without a token, 403 for a Read user on a write endpoint).

  Security

  - DTOs isolate the API contract; entities are never bound or returned directly,
  preventing over-posting (the price endpoint accepts only a price).
  - Parameterized SQL in the Dapper query (no injection).
  - JWT auth with policy-based role authorization.
  - Global exception handler returns RFC 7807 ProblemDetails with no leaked stack traces.
  - Security headers (X-Content-Type-Options, X-Frame-Options, Referrer-Policy).
  - Rate limiting on the login endpoint (brute-force protection).

  -----------------

  - 4-project layout (Data/Application/PublicAPI/Tests) instead of a stricter
  Domain/Infrastructure split — right altitude for this scope; avoids unwarranted ceremony.
  - Custom IValidator<T> + action filter instead of FluentValidation — the rule set is
  simple, so a dependency wasn't justified; the filter shows the ASP.NET pipeline is understood.
  - JWT HS256 with hardcoded demo users — self-contained for evaluation. Production would use
  asymmetric keys (RS256), a real user store, and hashed passwords (ASP.NET Identity / PBKDF2).
  - EF Core for the main read, Dapper for top-10 — as required; demonstrates both approaches.
  - Batched AddRange for the import — dependency-free and readable. For genuinely large
  loads I'd switch to SqlBulkCopy / EFCore.BulkExtensions to bypass change tracking.
  - Auto-migrate on startup — convenient for evaluation and tests; in production this would
  typically be a controlled deployment step.
  - No CQRS/MediatR/event sourcing — deliberately omitted; they'd add indirection without
  value at this scope.