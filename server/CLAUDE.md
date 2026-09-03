# server — .NET 8 Web API

Scope and stages: [`../IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md).
C# style: the [`dotnet`](.claude/skills/dotnet/SKILL.md) skill.

## Stack

C# 12 / .NET 8 Web API · EF Core 8 · SQL Server (PoC) · Serilog · FluentValidation · xUnit.

## Structure (plan §4)

```
SupportPlatform.sln
Directory.Build.props        net8.0 · nullable · TreatWarningsAsErrors — inherited by every project
src/
  Api             controllers, Swagger, Errors/ (IExceptionHandler + ProblemTypes), Middleware/ (correlation id)
  Application     use-case services, DTOs, validators; Search/ = QueryDefinition + FilterValue + validator + renderer + BucketPaging (sort/page result shaping)
  Domain          entities, value objects, FilterFieldRegistry — no framework refs
  Infrastructure  EF Core DbContext, repositories, migrations, seed; Search/ = DynamicQueryBuilder + Filters/ handlers + executor (data access only — no sort/page)
tests/
  Api.Tests             xUnit — endpoint tests via TestApiFactory (WebApplicationFactory<Program> + SQLite)
  Application.Tests      xUnit
  Infrastructure.Tests   xUnit — DbContext / seeder / tenant-filter tests over in-memory SQLite
  (one test project per src project; added as each layer gains code)
```

Project names are `SupportPlatform.<Layer>`; folders drop the prefix.
Dependencies point one way: `Api → Application → Domain`, `Infrastructure → Application → Domain`,
`Api → Infrastructure` (composition only). `Application` never references EF Core.
`QueryDefinition` is the single canonical object — form builds it, NL parser emits it,
saved query stores it, the SQL engine translates it.

## DI / composition

- Each layer exposes one `IServiceCollection` extension: `AddApplication()`, `AddInfrastructure()`.
- `Api/Program.cs` is the only composition root — it calls those extensions; nothing else wires services.
- `/health` is `AddHealthChecks()` + `MapHealthChecks("/health")` (returns `200 Healthy`).
- `Program` is declared `public partial` so the test host can boot it.

## Commands

Run from `server/`. Requires the .NET 8 SDK (`dotnet --version` → 8.x).

```bash
dotnet build SupportPlatform.sln          # warnings are errors
dotnet run --project src/Api               # http://localhost:5080 · Swagger at /swagger
dotnet test SupportPlatform.sln
curl http://localhost:5080/health          # -> 200 Healthy
```

Endpoints so far: `GET /api/metadata?tenantId=` (S1) · `POST /api/search` (S2 — body is a
`QueryDefinition`, response has `questionText` / `rows` / `aggregations` / `page` / `executionMeta`)
· `GET/POST/PUT/DELETE /api/saved-queries[/{id}]` + `POST /api/saved-queries/{id}/run` (S5 —
CRUD + re-run, scoped to owner + tenant; out-of-scope ⇒ 404).
Every request echoes an `X-Correlation-Id` header; errors are `application/problem+json`.

Caller identity (S5, PoC seam): `ICurrentUser` (`Application/Identity`) — `Username` / `TenantId`
/ `Role` / `CorrelationId`. Impl `HttpCurrentUser` (`Api/Identity`) reads the `X-User` header and
resolves the seeded `users` row; a missing/unknown header falls back to `sarah`. No JWT, no role
enforcement — that is S8. Send `X-User: <username>` from tests/clients to act as someone else.

Search dedup (S5): `SearchService` keys an `IMemoryCache` by the canonical `DefinitionHasher.Hash`;
a hit returns the stored `SearchResponse` with `executionMeta.cacheHit = true`. TTL from
`Search:CacheTtlSeconds` (default 60; `0` disables dedup — `TestApiFactory` sets `0` so repeated
identical posts stay deterministic).

Audit (S5): call `IAuditService.Record(action, entityType, entityId, payload)` **explicitly** from
the use-case service — never an EF interceptor. Wired on `search` and every saved-query mutation +
run. Rows carry `User` + `CorrelationId` from `ICurrentUser`; `payload` is stored as JSON.

### EF Core migrations

`dotnet-ef` is a local tool — run `dotnet tool restore` once. The design-time factory
(`Infrastructure/Persistence/DesignTimeDbContextFactory.cs`) lets Infrastructure act as its
own startup project, so the Api project stays free of `EFCore.Design`.

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project src/Infrastructure --startup-project src/Infrastructure --output-dir Persistence/Migrations
dotnet dotnet-ef database update       --project src/Infrastructure --startup-project src/Infrastructure
```

On `dotnet run` in **Development** only, `Program.cs` runs `Migrate()` then `DbSeeder.Seed()`
(needs SQL Server up — `cd ../infra && docker compose up db`). `DbSeeder` is deterministic and
idempotent; it no-ops if `support_requests` already has rows.

Migrations to date: `InitialCreate` · `TenantAndReferenceFkDeleteBehavior` · `SavedQueriesAndAudit`
(S5 — additive only: creates `saved_queries` + `audit_log`, touches no existing table).

Docker: `server/Dockerfile` publishes `src/Api` on the aspnet:8.0 runtime (listens on `8080`).
Full stack (db + api + client): `cd ../infra && docker compose up --build`.
The connection string is read from `ConnectionStrings:SqlServer` (env `ConnectionStrings__SqlServer` in Compose).

## Conventions

- One public type per file.
- DTOs cross the API boundary — never entities.
- EF Core lives only in `Infrastructure`.
- Validate input with FluentValidation.
- Inject dependencies through the constructor; depend on interfaces.
- Keep controllers thin — they call a service and return.
- Tenant-scoped entities (`SupportRequest`, `SubmittingBody`) carry a **fail-closed** global
  query filter via `ITenantContext`: no tenant set ⇒ zero rows. Reach past it only with an
  explicit `IgnoreQueryFilters()` (tests, admin, seeding's emptiness check).
- Seed/fixture passwords are stored as deterministic hashes (`SeedPasswordHasher`) — never
  plaintext. S1 has no auth logic; JWT lands in S8.
- Tests use SQLite (`TestApiFactory` for endpoints, `TestDb` in Infrastructure.Tests), not a
  real SQL Server.
- Errors: throw. The global `IExceptionHandler` (`Api/Errors`) maps `ValidationException` and
  `Application/Search/InvalidQueryException` to `400 validation`, everything else to `500`.
  Don't hand-build error responses. Correlation id via `CorrelationIdMiddleware`; Serilog console.
- Search request/response types live in `Application/Search/`; the FluentValidation validator in
  `Application/Search/Validation/`. `QueryDefinition.Filters` values are the closed `FilterValue`
  hierarchy with `FilterValueJsonConverter` (array = codes, `{type}` object = year).

## The query engine — extending it

- **New filter *value* (a domain, body type, district, …):** data only — reference rows + (if a
  whole new field) a `filter_field_registry` row + `DbSeeder`. No code.
- **New filter *field* over an existing kind:** one line in
  `Infrastructure/Search/Filters/FilterHandlers.Default` — `new CodeListFilterHandler("<id>",
  r => r.<column>)` — plus its registry + reference rows.
- **New *kind* of filtering (e.g. numeric range, text):** one new `FilterHandler` subclass.
  `DynamicQueryBuilder` and `FilterHandlerResolver` never change — no central `switch`.

## Don't

- Add features, layers, or patterns not in the plan.
- Reach across a layer or let `Application` touch EF. Keep business decisions in the Service, not
  the Controller and not Infrastructure (see the `dotnet` skill — "Responsibility boundaries").
- Build the dynamic query without a `FilterFieldRegistry` whitelist, or with a `switch` on field
  id, reflection, or string-parsed expressions.
