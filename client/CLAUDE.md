# client — React + TypeScript (Vite)

Scope and stages: [`../IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md).
Component patterns + worked table example: the
[`react-components`](.claude/skills/react-components/SKILL.md) skill.

## Stack

React + TypeScript + Vite · Ant Design (`ConfigProvider direction="rtl"`) ·
TanStack Query · Chart.js (`react-chartjs-2`) · Vitest.

## Structure (plan §4)

```
src/
  api/         http client, interceptor (ProblemDetails → notification), typed services
  components/  generic reusable components (DataTable, ChartCard, ...)
  features/
    search/  results/  saved-queries/  nl-query/
  state/       one hook + TanStack Query per feature
  models/      shared TypeScript types
```

## Commands

```bash
npm run dev
npm run build
npm test
npm run lint
```

## Style

- Small components. **One component per folder** — `Name/Name.tsx` + `index.ts`.
- Separate UI from logic: logic lives in hooks (`useX.ts`). **No business logic in JSX.**
- Data access in `src/api/` services returning typed results — components never call `fetch`.
- Types in `src/models/` (or co-located `*.types.ts`) — not inline.
- Generic components in `src/components/`; feature-only components under their feature folder.
- No `any`. Derive state, don't duplicate it.
- Every data view handles loading / empty / error.
