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
  Api             controllers, Swagger, auth, ProblemDetails, request validation
  Application     use-case services, DTOs, validators, QueryDefinition, NL translator
  Domain          entities, value objects, FilterFieldRegistry — no framework refs
  Infrastructure  EF Core DbContext, repositories, DynamicQueryBuilder, migrations, seed
tests/
  Api.Tests           xUnit — endpoint tests via WebApplicationFactory<Program>
  Application.Tests    xUnit
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

## Conventions

- One public type per file.
- DTOs cross the API boundary — never entities.
- EF Core lives only in `Infrastructure`.
- Validate input with FluentValidation.
- Inject dependencies through the constructor; depend on interfaces.
- Keep controllers thin — they call a service and return.

## Don't

- Add features, layers, or patterns not in the plan.
- Reach across a layer or let `Application` touch EF.
- Build the dynamic query without a `FilterFieldRegistry` whitelist.
