---
description: Drive one IMPLEMENTATION_PLAN.md stage (S0–S11) end to end — confirm the stage, open a worktree via /new-task, assemble the task card + plan, stop for approval, then implement through the DoD gate to an open PR.
argument-hint: "[stage e.g. S1 or S0-e — optional]"
---

# build-stage

Run the per-stage loop defined in `IMPLEMENTATION_PLAN.md` §9 for a **single** stage.
One invocation = one stage = one PR. Never start the next stage in the same run.

The plan (§3 working agreement, §5 contracts, §6 stages, §7 fallback ladder, §9 loop,
§10 card template) is the source of truth. Build only what the stage lists; when two
ways exist, take the simpler one; do not invent requirements.

Requested stage (may be empty): `$ARGUMENTS`

## 1 · Resolve the stage

- If a stage was passed above (`S2`, `S0-e`, …), use it.
- Otherwise consult the auto-memory `project-status` entry (via the `MEMORY.md` index
  already loaded into context — **do not hardcode any `C:\Users\...` path**; go through
  the memory mechanism). Propose the current/next stage to the user
  ("next up: **S1 — data model + metadata + seed**") and wait for them to confirm or
  name a different one. Accept sub-stages such as `S0-e`.
- Read the matching stage block in §6, plus §3, §3 item 8 (DoD), §5, §7, §10. Re-read
  root `CLAUDE.md`, `server/CLAUDE.md`, `client/CLAUDE.md`.

## 2 · Sanity check (§9.1)

Bring the stack up and confirm it is healthy before writing anything:

```bash
cd infra && docker compose up -d --build
```

Verify `/health` returns 200 and the client loads. If something an earlier stage built
is broken, fix that first or stop and report — do not build on a broken base. The user
may reply "skip" to bypass a slow container rebuild when they know it is green.

## 3 · Open the worktree

Use the existing **`/new-task` skill exactly as its own instructions describe**, passing
a slug derived from the stage (`s1-data-model`, `s2-query-engine`, …). Do **not** modify
`new-task.ps1`, re-implement its worktree logic, or add another worktree mechanism.
Report the `worktree:` / `branch:` lines it prints. If it errors (path or branch exists,
etc.), show the message and stop. All further work happens inside that worktree.

## 4 · Assemble the card + plan — then STOP

Fill the §10 task-card template for this stage from the plan text:

- context (2–3 lines: where it sits in the architecture, why it matters)
- dependencies that must already be done
- inputs — always attach `docs/contracts/*` (query-definition, api-contract,
  metadata-model, error-model); plus the existing files/modules it touches
- deliverable — exact files / classes / endpoints
- tests — which ones
- doc update — which `ARCHITECTURE.md` / `DESIGN_QA.md` chapter (or none)
- which `CLAUDE.md` (server or client) will need new commands/conventions

Then draft an ordered implementation plan: the steps, vertical-slice-first ordering
(§3.3), which chunks (if any) go to sub-agents (§5 below), and — if the stage is
time-boxed — the fallback option from §7.

**Present the card and plan, then halt. Do not edit, create, or run any non-read-only
command until the user replies with an explicit go.** On changes requested, revise and
re-present. On "go", continue.

## 5 · Implement (only after approval)

Work the deliverables in dependency order, **vertical slice first**
(`metadata → QueryDefinition → /search → results` runs end to end before anything else).

Sub-agents — be conservative, especially for S1 and S2:

- **`Explore`** — read-only investigation (existing patterns, contract details,
  conventions). Safe to run several in parallel.
- **`general-purpose`** — only for **clearly independent** implementation chunks with a
  crisp, self-contained spec. Never run parallel agents that touch the same entity,
  contract, or architectural area. When in doubt, do it serially in the main session.
- Apply the **`dotnet`** skill for `server/**`, the **`react-components`** skill for
  `client/**`.

### Stop and ask the user — do not decide these autonomously

- `IMPLEMENTATION_PLAN.md` conflicts with the existing code or another contract.
- An architectural or product decision is required that the plan does not define.
- A database migration could delete, alter, or otherwise risk existing data.
- A frozen S0 contract (`docs/contracts/*`) would need to change.
- A build or test failure remains after two reasonable fix attempts.
- The work would require scope beyond the current stage.

## 6 · DoD gate (§3 item 8) — strict order, never reordered

1. implementation complete
2. `dotnet build` (server) / `npm run build` (client) — green
3. `dotnet test` / `npm test` — focused tests green
4. update the relevant `ARCHITECTURE.md` / `DESIGN_QA.md` chapter, and
   `server/CLAUDE.md` or `client/CLAUDE.md` with any new commands/conventions
5. run `/code-review medium`
6. fix every correctness finding
7. **re-run build + tests** — still green
8. no secrets, no dead code; stack still boots

Never commit before step 8 is green.

## 7 · Commit + PR

Only once the gate is fully green:

- one Conventional Commit tagged with the stage —
  `feat: data model + metadata + seed (S1)` / `fix:` / `docs:` / `test:` as fits
- push the branch
- open a PR against `main`; body lists the deliverables and ticks the DoD checklist
- one stage = one PR — report the PR URL

## 8 · Update memory

Through the memory mechanism, update the `project-status` entry: new current stage, and
note the PR is pending review.

## Guardrails

- Never skip the step 4 stop.
- Never start the next stage in the same run.
- Never commit before the step 6 gate is green.
- If `/new-task` or the sanity check errors, show the message and stop — don't work
  around it.
