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
  api/         http.ts (fetch wrapper + ProblemDetails→notification interceptor), config.ts, typed services
  components/  generic reusable components (PagePlaceholder, DataTable, ...)
  features/
    search/  results/  saved-queries/  nl-query/
      each: components in <Component>/ folders, hooks in hooks/, feature-local helpers alongside
  state/       queryClient.ts — the shared TanStack Query client only (per-feature hooks live under the feature)
  models/      shared TypeScript types (metadata, queryDefinition, search, problemDetails)
test/setup.ts  Vitest setup — RTL cleanup + ResizeObserver/matchMedia stubs for antd
```

Search results are shown **inline on `SearchPage`** — there is no `/results` route (removed in S3;
S7 may reintroduce a dedicated view with the chart). `features/results/` still holds the results
components (`ResultsTable`, `ResultsPanel`, `QuestionPanel`) and `useSearch`.

## Routing

- Routes are declared once in `src/App/routes.tsx` as `{ path, label, element }[]`; `App.tsx`
  renders both the nav `Menu` and the `<Route>`s from that array.
- `/` and any unknown path redirect to the first route (`/search`).
- Dev server runs on **port 5173** (`vite.config.ts`), `host: true` so it works in Docker.
- `/api/*` is proxied to the api (`VITE_API_PROXY_TARGET`, default `http://localhost:5080`).

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

## API seam (`src/api/`)

- `http.get` / `http.post` are the only callers of `fetch`. On a non-2xx response `http.ts` parses
  RFC 7807 ProblemDetails, raises one `notification.error` (title + detail + `traceId`), and throws
  `ApiError` — hooks/components surface it from that, they don't re-notify.
- One service per resource (`metadataApi`, `searchApi`), each returning a `models/` type.
- `DEFAULT_TENANT_ID` (`api/config.ts`) is a temporary stand-in until auth lands in S8 — do not
  scatter tenant literals elsewhere.

## The search slice

- The form is generated from `GET /api/metadata` `filterFieldRegistry` — one control per entry, in
  array order. Never hard-code a filter field, label, or option list in a component.
- `buildQueryDefinition` (pure) is the single place the form → `QueryDefinition` mapping lives; unit
  test it there.
- Paging and sorting are **server-side**: `ResultsTable` translates antd `Table.onChange` into
  `QueryDefinition.paging` / `.sort` and refetches; `SearchResponse.page.totalRows` drives the pager.
- `questionText` always comes from the server (`POST /api/search`) — no client-side Hebrew renderer.
