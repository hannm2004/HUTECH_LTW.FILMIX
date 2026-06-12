# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

FILMIX — a Netflix-clone movie streaming web app built as a HUTECH "Lập Trình Web" course project. ASP.NET Core 9 MVC + EF Core 9 (code-first), ASP.NET Core Identity, with a parallel REST API layer documented via Swagger. UI strings, comments, and seeded data are in Vietnamese.

## Commands

```powershell
dotnet run            # build + run; serves at http://localhost:5241 (Swagger at /swagger in Development)
dotnet build
dotnet watch run      # hot reload during development
```

There is **no test project** — do not look for or invent test commands.

EF Core migration tooling (note the schema caveat below before using):
```powershell
dotnet ef migrations add <Name>
dotnet ef database update
```

## Critical gotchas

- **The root namespace is `untitled1`** (project file is `untitled1.csproj`), not `Filmix`. All `using` statements and namespace declarations use `untitled1.*` (e.g. `untitled1.Services`, `untitled1.Models.Entities`). Keep this when adding files — do not "fix" it to FILMIX.
- **Schema is created with `EnsureCreated()`, not migrations.** `Program.cs` calls `db.Database.EnsureCreated()` on startup. Despite the populated `Migrations/` folder, migrations are effectively **not applied at runtime**. Consequence: editing entity classes or the `OnModelCreating`/`HasData` seed in `ApplicationDbContext` has **no effect on an existing database** — you must **drop the database** (e.g. `DROP DATABASE filmix_db;`) and re-run so `EnsureCreated()` rebuilds it. See the seeded sample data (movies, categories, plans) inside `Data/ApplicationDbContext.cs`.
- **Seed data lives in two places**: static reference data (movies, categories, episodes, subscription plans, movie images) is in `ApplicationDbContext.OnModelCreating` via `HasData`; roles (`Admin`/`User`) and admin user accounts are seeded imperatively in `Data/DbSeeder.cs` (idempotent, runs every startup). Default admins: `admin1@filmix.com` / `admin2@filmix.com`, password `admin@123`.

## Database provider switch

`appsettings.json` selects the provider at runtime via the top-level `"DbProvider"` key — `"MySql"` (default, via `Pomelo.EntityFrameworkCore.MySql`) or `"SqlServer"`. `Program.cs` branches on this to call `UseMySql`/`UseSqlServer` with `DefaultConnection`. Current local config targets MySQL (`root`/`123456`, db `filmix_db`).

## Architecture

Request flow is layered: **Controller → Service → Repository → `ApplicationDbContext`**, all wired through constructor DI registered in `Program.cs`.

- **Controllers/** — MVC controllers returning Razor views (`HomeController`, `ProductController`, `SearchController`, `WatchlistController`, etc.) AND REST API controllers (suffix `*ApiController`: `AuthApiController`, `CartApiController`, `ProductsApiController`).
- **Areas/Admin/** — admin dashboard (Dashboard, Analytics, Order, Product, Subscription, SystemLog, User controllers + views). Default area route lands on `Dashboard/Index`.
- **Services/** — business logic behind `I*Service` interfaces: `CartService` (session-based cart), `OrderService` (checkout + Premium activation), `AdminService`, `RecommendationService` (personalized recs from viewing history), `LogService` (writes `SystemLog` audit rows), `JwtService` (token issuance).
- **Repositories/** — thin EF data-access classes behind `I*Repository` interfaces (Order, SubscriptionPlan, User, Subscription, ViewingHistory, Log). Repositories are the only layer that should touch `DbSet`s directly; controllers/services go through them (except where services hold `ApplicationDbContext` for read-heaviness like recommendations).
- **Models/** — `Entities/` (EF entities; `Entities.cs` holds Movie/Episode/Category/Order/etc., `ApplicationUser.cs` extends `IdentityUser`), `DTOs/` (API request/response, including shared `ApiResponse<T>`), `ViewModels/` (admin + cart views), `Settings/JwtSettings.cs`.
- **ViewComponents/** — e.g. `HeroBannerViewComponent`.
- **wwwroot/** — vanilla CSS/JS (no build step, no npm). Slider, hero banner, continue-watching, and auth-state are plain `.js` files.

### Dual authentication

The app runs **two independent auth schemes simultaneously**:
1. **Cookie auth** (ASP.NET Identity) for the MVC site — login at `/Account/Auth`.
2. **JWT Bearer** for the `*ApiController` REST endpoints — config in `JwtSettings` (appsettings.json), tokens issued by `JwtService`. JWT failures return JSON (custom `OnChallenge`/`OnForbidden` events in `Program.cs`), never HTML redirects.

Model-validation failures on API endpoints are reformatted into the shared `ApiResponse<T>` envelope (see `ConfigureApiBehaviorOptions` in `Program.cs`). Requests under `/api` skip the HTML `UseStatusCodePagesWithReExecute` error re-execution.

### Swagger

Three grouped docs — `auth`, `cart`, `products` — routed by controller-name matching in `DocInclusionPredicate`. New API controllers must contain `AuthApi`/`CartApi`/`ProductsApi` in their type name to appear in a group, or extend the predicate. Swagger is **Development-only**.

## Conventions

- Comments, log messages, validation messages, and seed content are written in **Vietnamese** — match this when editing existing files.
- API responses use the shared `ApiResponse<T>` wrapper (`Models/DTOs`) with `success`/`message`/`data`. Reuse it rather than returning raw objects.
- Cart is **cookie-based** (`CartService` stores a Base64-encoded JSON cart in the persistent `FilmixCart` cookie, 30-day expiry). It survives login/logout and app restarts, and guests can add to cart; login is enforced only at checkout (`OrderController` is `[Authorize]`). Cart is cleared (cookie deleted) when checkout creates an `Order`.
- **Watchlist ("Danh Sách Của Tôi")** is DB-backed per user via the `WatchlistItems` table (`WatchlistItem` entity: `UserId`, `MovieId`, `AddedAt`; unique on `UserId,MovieId`). `localStorage['filmix_watchlist']` is only a client cache: `wwwroot/js/watchlist-sync.js` (loaded in `_Layout <head>`) hydrates it from server-rendered `window.FILMIX_WATCHLIST` on load and mirrors every change to `POST /api/watchlist/sync` (full-list replace). `GET /api/watchlist/ids` returns the current user's IDs; `GET /api/watchlist?ids=` returns movie details for rendering. Logout clears the localStorage cache (via `filmixClearUserData()`) but keeps the DB rows. NOTE: the `WatchlistItems` table already exists in the local MySQL DB (pre-dates the current entity), so `EnsureCreated` is not needed for it.
- Two parallel payment flows exist: the **cart/order flow** (`Cart` → `Order/Checkout` → `Order/Payment` → `Order/Success`, the one wired to the UI) and an **orphaned `Subscription/Checkout`** 3D-card flow (`ProcessPayment`) not linked from any page. Premium activation happens in `OrderService.ActivatePremiumSubscriptionAsync` via `Order/ProcessMockPayment`.
