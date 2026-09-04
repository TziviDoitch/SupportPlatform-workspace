# SupportPlatform — English Overview

A Proof of Concept for a **cross-organization government support-request system**:
an office employee defines search criteria, reads the results as a table and a chart,
saves frequent queries, and can ask the same question in free-text Hebrew.

> This document is a concise English overview of the project. It is not a translation
> of the Hebrew documentation. The Hebrew documentation remains the authoritative
> source for detailed requirements and implementation decisions — see
> [Where to read more](#where-to-read-more).

---

## Stack

.NET 8 Web API (C#) · EF Core 8 · SQL Server · React 19 + TypeScript (Vite) ·
Ant Design (RTL) · TanStack Query · Chart.js · Serilog · FluentValidation · xUnit + Vitest.

## Run it

```bash
cd infra
cp .env.example .env      # set MSSQL_SA_PASSWORD
docker compose up --build
```

| Service | URL |
|---|---|
| API | http://localhost:5080 — `/health`, Swagger at `/swagger` |
| Client | http://localhost:5173 |
| DB | `localhost:1433` (SQL Server 2022) |

Migrations and deterministic seed data run automatically on startup in Development.
Without Docker on Windows: `./run-local.ps1` (uses LocalDB).

There is no login screen in the PoC. Identity is the `X-User` header, defaulting to
the seeded user `sarah`. Seeded users: `sarah` (analyst) and `dan` (admin) in tenant
`culture-sport-admin`, `michal` (analyst) in `welfare-admin`.

## Architecture in one screen

Four backend layers, dependencies pointing one way, `Application` never references EF Core:

```
Api             thin controllers, Swagger, RFC 7807 ProblemDetails, correlation-id middleware
Application     use-case services, DTOs, validators, QueryDefinition, QuestionTextRenderer,
                INlQueryProvider (the AI seam)
Domain          entities, FilterFieldRegistry — no framework references
Infrastructure  EF Core DbContext, repositories, DynamicQueryBuilder, migrations, seed
```

**The central idea:** `QueryDefinition` is a single canonical object. The form builds it,
the natural-language parser emits it, a saved query *is* it, the SQL engine translates it,
and the question renderer reads it. One shape, five consumers.

**Safety by construction:** `DynamicQueryBuilder` composes an `IQueryable` only from fields
present in the `filter_field_registry` whitelist. A field absent from the registry is
rejected before any handler runs — no `switch` on field id, no reflection, no
string-parsed expressions.

**Extensibility, in three sizes:**

| Change | Cost |
|---|---|
| New value for an existing field (a new support domain, status, district) | **Data only — zero code** |
| New filter field over an existing kind | One registration line |
| New *kind* of filtering (numeric range, free text) | One handler subclass |

The first case is demonstrated end-to-end in `EXTENSIBILITY_DEMO.md`.

**Multi-tenancy is real, not theoretical.** `TenantId` sits on `SupportRequest`,
`SubmittingBody`, `SavedQuery` and `User`, behind a **fail-closed** EF global query
filter — with no tenant in scope, queries return zero rows, never everything. A guard
returns `403` for a foreign tenant, and two ministries are seeded with their own users
and data so the isolation is actually testable.

## API

| Method | Path |
|---|---|
| `GET` | `/api/metadata` — reference lists + filter registry; drives the dynamic form |
| `POST` | `/api/search` — runs a `QueryDefinition` |
| `GET/POST/PUT/DELETE` | `/api/saved-queries[/{id}]` |
| `POST` | `/api/saved-queries/{id}/run` |
| `POST` | `/api/nl-queries/parse` — free text → definition + interpretation + unresolved terms |
| `GET` | `/health` |

Every response carries `X-Correlation-Id`; every error is `application/problem+json`.

## Scope — what is built and what is not

**Implemented and running:** the query engine (filters, year ranges, segmentation,
aggregation), readable question rendering, results table and chart, saved-query CRUD
and re-run, Hebrew natural-language parsing behind a swappable provider interface,
audit log, query deduplication by canonical hash, tenant isolation, and a role check.
214 automated tests (158 server, 56 client).

**Deliberately not implemented:**

| Item | Why |
|---|---|
| Real authentication (JWT / IdP) | Production target; the PoC uses an `X-User` seam |
| Raw record listing (`resultKind: "list"`) | Would change a frozen contract; aggregation covers the need |
| CI/CD, automated deployment, IaC | The assignment states DevOps needs description only, not implementation |
| Per-tenant metadata | Reference tables are keyed by `Code` alone; true isolation needs a composite key and four composite foreign keys — a model change, not a column |
| External LLM provider | The seam exists and is config-selected; the shipped provider is deterministic and needs no API key |

Nothing above is an oversight — each is a scoping decision, recorded with its reasoning.

## Where to read more

All authoritative documentation is in Hebrew:

| Document | Contents |
|---|---|
| [`../README.md`](../README.md) | Run instructions, technology choices, project structure, assumptions, limitations, and a **requirement-coverage matrix** mapping every assignment requirement to its proof |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | Layers, modules, the query engine, database model, client, ERD and container diagrams, and a **14-entry Decision Log** |
| [`DESIGN_QA.md`](DESIGN_QA.md) | The eight design questions: extensibility, multi-tenancy, permissions, heavy queries, deduplication, multiple AI providers, observability, shared infrastructure |
| [`DEVOPS.md`](DEVOPS.md) | Environments, CI/CD, secrets, configuration, deployment — design only |
| [`TEST_PLAN.md`](TEST_PLAN.md) | Manual test scenarios and edge cases |
| [`contracts/`](contracts/) | Frozen contracts: `query-definition` (+ JSON schema), `api-contract`, `metadata-model`, `error-model` |

Developer-facing guides are already in English: [`../server/CLAUDE.md`](../server/CLAUDE.md)
and [`../client/CLAUDE.md`](../client/CLAUDE.md) cover build commands, structure and conventions.
