---
name: dotnet
description: C# / .NET conventions for the SupportPlatform server project. Apply on every .cs edit under server/ — writing or changing any C# in Api, Application, Domain, or Infrastructure. בעברית: עבודה בקוד C# / .NET בצד השרת.
---

# C# Conventions — server

Light rules for a project with no code yet. Architecture first, then style.
Stages and the canonical `QueryDefinition` are defined in
[`../../../IMPLEMENTATION_PLAN.md`](../../../IMPLEMENTATION_PLAN.md) — build one stage at a time.

## Architecture

- Four layers, one-way dependencies: `Api → Application → Domain`, `Infrastructure → Application/Domain`.
- `Domain` has no framework references. EF Core only in `Infrastructure`.
- `Application` holds use-case services, DTOs, validators. It never sees `DbContext`.
- Controllers are thin: bind, call one service, return.
- The dynamic query builder accepts only fields in the `FilterFieldRegistry` whitelist.

## Responsibility boundaries (applies to every stage)

- **Business logic lives in the Service layer** (`Application/**/*Service.cs`). The application
  flow and every business decision a use case needs — filtering decisions, aggregation,
  segmentation, domain calculations, authorization decisions, response shaping — belong there.
- **Controllers are HTTP-only:** receive the request, do basic model binding, call one service,
  return the HTTP result. No business rules, no filtering/aggregation/segmentation logic, no
  authorization decisions, no domain math in a controller.
- **Infrastructure is data access only:** EF queries, the `DynamicQueryBuilder` + filter
  handlers, tenant-scope mechanism, migrations, seed. It must not host application/business
  orchestration. Applying a decision the service already made (e.g. scoping to a validated
  tenant id) is a data-access mechanism and stays here; *making* the decision does not.
- If logic is duplicated or naturally belongs in the service, move it to the service rather
  than the controller. Do not push logic into Infrastructure just to thin a controller.
- Do not add abstractions whose only purpose is to enforce this separation — keep it simple
  and consistent with the layers above.

## Style

- File-scoped namespaces: `namespace SupportPlatform.Application;`
- Expression-bodied members for one-liners: `public int Count => _rows.Count;`
- Null checks by pattern matching: `if (x is null)`, `if (x is not null)` — not `== null`.
- Short names. Don't encode the type in the method name; don't suffix `ById` when it's unambiguous
  (`Get(int id)`, not `GetById(int id)`).
- No ceremonial comments — comment only a non-obvious decision.
- Small, single-responsibility methods. Extract a private method when a pattern repeats in a file.
- Constructor injection; depend on interfaces. Interface files go in an `Interfaces/` subfolder
  next to their implementation (`Services/Interfaces/ISearchService.cs`).

## Base practices

- One public type per file. One stage per PR, Conventional Commits.
- Add a focused xUnit test with each unit of logic (builder filters, aggregation, renderer).
- No secrets in code or committed config — use `appsettings.*.local.json` / env vars.
- New `.cs` files: UTF-8, no BOM.

## Cross-cutting (landed in S2)

- Errors: throw; the global `IExceptionHandler` (`Api/Errors`) maps to RFC 7807 ProblemDetails
  (`docs/contracts/error-model.md`). `FluentValidation.ValidationException` and the
  Application `InvalidQueryException` become `400`; anything else is `500`. Don't catch to
  build error responses by hand in controllers or services.
- Logging: Serilog console, injected `ILogger<T>`. Every request carries a correlation id
  (`X-Correlation-Id`, echoed on the response, emitted as `traceId` in ProblemDetails and on
  every log line). Don't `Console.WriteLine`.
- Input validation: FluentValidation on `QueryDefinition`, checked in the service before use.
