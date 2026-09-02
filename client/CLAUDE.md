# client — React + TypeScript (Vite)

Scope and stages: [`../IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md).
Component patterns + worked table example: the
[`react-components`](.claude/skills/react-components/SKILL.md) skill.

## Stack

React 19 + TypeScript + Vite 8 · Ant Design v6 (`ConfigProvider direction="rtl"`) ·
react-router-dom v7 · TanStack Query v5 · Chart.js (`react-chartjs-2`, added in S7) ·
Vitest + `@testing-library/react` + jsdom · lint via **oxlint** (not ESLint).

antd v6 is CSS-in-JS — no stylesheet import. RTL comes from `<html dir="rtl">` in
`index.html` plus the `ConfigProvider`.

## Structure (plan §4)

```
src/
  main.tsx     entry — providers: QueryClientProvider > ConfigProvider(rtl, he_IL) > BrowserRouter > App
  App/         app shell — Layout + Menu nav + <Routes>; routes.tsx is the route list (data)
  api/         http client, interceptor (ProblemDetails → notification), typed services
  components/  generic reusable components (PagePlaceholder, DataTable, ...)
  features/
    search/  results/  saved-queries/  nl-query/    (each: <Feature>Page/ + index.ts)
  state/       queryClient.ts (shared client) + one hook + TanStack Query per feature
  models/      shared TypeScript types
test/setup.ts  Vitest setup — RTL cleanup + ResizeObserver/matchMedia stubs for antd
```

## Routing

- Routes are declared once in `src/App/routes.tsx` as `{ path, label, element }[]`; `App.tsx`
  renders both the nav `Menu` and the `<Route>`s from that array.
- `/` and any unknown path redirect to the first route (`/search`).
- Dev server runs on **port 5173** (`vite.config.ts`).

## Commands

```bash
npm run dev          # http://localhost:5173
npm run build        # tsc -b && vite build
npm test             # vitest run
npm run test:watch
npm run lint         # oxlint
```

## Style

- Small components. **One component per folder** — `Name/Name.tsx` + `index.ts`.
- Separate UI from logic: logic lives in hooks (`useX.ts`). **No business logic in JSX.**
- Data access in `src/api/` services returning typed results — components never call `fetch`.
- Types in `src/models/` (or co-located `*.types.ts`) — not inline.
- Generic components in `src/components/`; feature-only components under their feature folder.
- No `any`. Derive state, don't duplicate it.
- Every data view handles loading / empty / error.
