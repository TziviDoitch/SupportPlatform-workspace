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
  Application     use-case services, DTOs, validators; Search/ = QueryDefinition + FilterValue + validator + renderer + BucketPaging (sort/page result shaping); NlQuery/ = the AI seam + rule-based parser
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

Endpoints so far: `GET /api/metadata?tenantId=` (S1; S8 ⇒ 403 if `tenantId` ≠ caller's) ·
`POST /api/search` (S2 — body is a `QueryDefinition`, response has `questionText` / `rows` /
`aggregations` / `page` / `executionMeta`; S8 ⇒ 403 on `tenantId` mismatch)
· `GET/POST/PUT/DELETE /api/saved-queries[/{id}]` + `POST /api/saved-queries/{id}/run` (S5 —
CRUD + re-run, scoped to owner + tenant; out-of-scope ⇒ 404; S8 — DELETE requires role `admin` ⇒ 403).
· `POST /api/nl-queries/parse` (S6 — free text ⇒ `{ definition, interpretationText, confidence,
unresolved }`; S8 ⇒ 403 on `tenantId` mismatch).
Every request echoes an `X-Correlation-Id` header; errors are `application/problem+json`.
`src/Api/SupportPlatform.Api.http` has a ready request per endpoint (S7); Swagger UI at `/swagger`
in Development lists them all.

Caller identity (S5 seam, S8 authoritative): `ICurrentUser` (`Application/Identity`) — `Username` /
`TenantId` / `Role` / `CorrelationId`. Impl `HttpCurrentUser` (`Api/Identity`) reads the `X-User`
header and resolves the seeded `users` row; a missing/unknown header falls back to `sarah`. Send
`X-User: <username>` from tests/clients to act as someone else. **No JWT / `/api/auth/login`** —
that stays the production target (`docs/ARCHITECTURE.md` §8.1, decision 13).

Authorization (S8):
- **Tenant is authoritative from identity.** `TenantAccessGuard.EnsureTenant(requestedTenantId)`
  (`Application/Identity`) — null/blank ⇒ the caller's tenant; a different tenant ⇒
  `ForbiddenException` (403 `forbidden`). Called by `SearchService`, `MetadataService`,
  `NlQueryService`. `?tenantId=` / `definition.tenantId` / `NlParseRequest.tenantId` are validated,
  not trusted. The tenant global query filter is the second line of defence.
- **One role rule:** deleting a saved query requires role `Roles.Admin` (`Application/Identity/Roles`),
  checked in `SavedQueryService.Delete` **after** the owner/tenant scope check — an out-of-scope id
  stays 404, an in-scope analyst gets 403.
- Throw `ForbiddenException` (`Application/Common`) for any "authenticated but not allowed"; the
  global handler maps it to 403. Saved-query out-of-scope access stays `NotFoundException` ⇒ 404.

Data access is through repositories (`Infrastructure/Repositories/`), not direct `DbContext`:
- `IRepository<T>` (`Application/Common/Interfaces`) — read-only (`ListAllAsync`) for a small set
  loaded whole; `TenantRepository` implements it. **No generic write abstraction, no `EfRepository<T>`
  base** (decision 14).
- `ISupportRequestRepository.Query()` returns the no-tracking `IQueryable<SupportRequest>` the
  search engine composes filters onto (replaces the S2 `DbContext` injection in `SearchQueryExecutor`).
- `MetadataRepository` / `SavedQueryRepository` (S1/S5) keep their own purpose-built interfaces.
- Add a specific repository only when a service needs data access the existing ones don't cover;
  don't widen `IRepository<T>`.

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
  real SQL Server. Unit tests cover the query engine, `QuestionTextRenderer`, the NL parser and
  `DefinitionHasher`; `Api.Tests/HappyPathIntegrationTests` is the single end-to-end chain
  (metadata → search → save → run → nl-parse). Manual scenarios + edge cases live in
  `docs/TEST_PLAN.md` — keep its "covered automatically" table in sync when you add a named edge test.
- Errors: throw. The global `IExceptionHandler` (`Api/Errors`) maps `ValidationException` and
  `Application/Search/InvalidQueryException` to `400 validation`, everything else to `500`.
  Don't hand-build error responses. Correlation id via `CorrelationIdMiddleware`; Serilog console.
- Search request/response types live in `Application/Search/`; the FluentValidation validator in
  `Application/Search/Validation/`. `QueryDefinition.Filters` values are the closed `FilterValue`
  hierarchy with `FilterValueJsonConverter` (array = codes, `{type}` object = year).

## The NL layer (S6) — the AI seam

`Application/NlQuery/`. `INlQueryProvider` is the **only** AI boundary:
`Translate(text, tenantId, SearchMetadata) → NlTranslation`.

- **`Translate` is the seam; `Parse` is the API use case.** The endpoint and `INlQueryService`
  keep the contract's `parse` wording (`api-contract.md` §4); the provider interface does not,
  because parsing is how *this* implementation works, not what the abstraction promises. The flow
  is `API Parse → NlQueryService → provider Translate → QueryDefinition`. Don't rename either half
  to match the other.
- A provider **translates and nothing else** — no `DbContext`, no search, no validation. The
  metadata is passed in as an argument so it stays free of data access.
- `NlQueryService` does the rest: it runs the produced definition through the existing
  `IValidator<QueryDefinition>` (a provider is never trusted), renders `interpretationText` with
  the existing `QuestionTextRenderer`, and audits `nl-parse`. **Parsing never runs a search** —
  `POST /api/search` stays the only execution path.
- **Selection is configuration, not a recompile.** `AddApplication()` holds a
  `provider key → type` map, registers each via **keyed DI** (`AddKeyedScoped` — .NET 8 built-in,
  no hand-rolled factory), and registers `INlQueryProvider` as a resolver that reads
  `NlQueryOptions.Provider`. The value comes from `NlQuery:Provider` in `appsettings.json`
  (default `ruleBased`). **Adding a provider = one map entry + one config value.** An unknown key
  fails at startup (`Program.cs` resolves the provider once after `Build()`), never as a 500 on
  the first question. `DependencyInjectionTests` locks all three paths.
- `RuleBasedNlQueryProvider` (key `ruleBased`) is the PoC implementation: deterministic, no
  external LLM, no API key (`DESIGN_QA.md` §6).
- The rules in `RuleBased/Rules/` are keyed off **metadata, never business values**:
  `CodeListFilterRule` covers every `codeList` registry field at once, so a new domain/status/
  district in the seed is recognised with no code change. `HebrewText` does the crude stemming —
  strip one ending, then attached particles; both sides of a comparison go through it, so the
  stems only have to be *consistent*, not linguistically right. It is **deliberately limited**:
  there is no viable Hebrew NLP library for .NET 8 (`ARCHITECTURE.md` §10 decision 12), and forms
  it cannot reduce surface in `unresolved` rather than being guessed at. Don't reach for a
  morphology dependency without revisiting that decision.
- Never invent a value, and never swallow one. A word that cannot be mapped goes to `unresolved`;
  an ambiguous one (a label word shared by two fields) resolves to nothing. A field **named** in
  the question counts as understood only if it was actually used, so "לפי סטטוס" (not segmentable)
  is reported instead of silently dropped. `confidence` is an indication only.

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
