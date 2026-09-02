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

## Not yet in scope

No "never try-catch" or "no logger" bans — the global error handler and centralized logging
don't exist yet. Revisit these rules once that infrastructure lands (S2).
