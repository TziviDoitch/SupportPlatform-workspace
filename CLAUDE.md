# SupportPlatform

Take-home PoC: a cross-organization system to store and query government support
requests. .NET 8 Web API + React/TypeScript client, run via Docker Compose.

## Source of truth

[`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) defines everything: locked tech
decisions (§2), the working agreement (§3), and the S0–S11 build order (§6).

- Build only what the plan lists. Do not invent requirements.
- When two ways exist, pick the simpler one.
- One stage per PR. Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`).

## Layout

```
server/   .NET 8 solution — Api / Application / Domain / Infrastructure
client/   React + TypeScript (Vite)
docs/     ARCHITECTURE.md, DESIGN_QA.md, contracts/
infra/    docker-compose + DevOps
```

## Style

Write short code: small units, one job each, no layers or patterns beyond the plan.
Per-project commands and conventions: [`server/CLAUDE.md`](server/CLAUDE.md),
[`client/CLAUDE.md`](client/CLAUDE.md).
