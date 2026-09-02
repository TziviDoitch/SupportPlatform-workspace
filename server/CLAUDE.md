# server — .NET 8 Web API

Scope and stages: [`../IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md).
C# style: the [`dotnet`](.claude/skills/dotnet/SKILL.md) skill.

## Stack

C# 12 / .NET 8 Web API · EF Core 8 · SQL Server (PoC) · Serilog · FluentValidation · xUnit.

## Structure (plan §4)

```
Api             controllers, Swagger, auth, ProblemDetails, request validation
Application     use-case services, DTOs, validators, QueryDefinition, NL translator
Domain          entities, value objects, FilterFieldRegistry — no framework refs
Infrastructure  EF Core DbContext, repositories, DynamicQueryBuilder, migrations, seed
Tests           xUnit
```

Dependencies point one way. `Application` never references EF Core.
`QueryDefinition` is the single canonical object — form builds it, NL parser emits it,
saved query stores it, the SQL engine translates it.

## Commands

```bash
dotnet build
dotnet run --project Api
dotnet test
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
