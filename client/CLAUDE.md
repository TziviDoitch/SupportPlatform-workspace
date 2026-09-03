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

- `http.get` / `http.post` / `http.put` / `http.del` are the only callers of `fetch`. On a non-2xx
  response `http.ts` parses RFC 7807 ProblemDetails, raises one `notification.error` (title + detail
  + `traceId`), and throws `ApiError` — hooks/components surface it from that, they don't re-notify.
- Every request carries an `X-User` header (`DEFAULT_USER`, `api/config.ts`) — the PoC identity seam
  until S8. `http.post` body is optional (for `POST .../run`).
- One service per resource (`metadataApi`, `searchApi`, `savedQueriesApi`), each returning a
  `models/` type.
- `DEFAULT_TENANT_ID` / `DEFAULT_USER` (`api/config.ts`) are temporary stand-ins until auth lands in
  S8 — do not scatter tenant/user literals elsewhere.

## The search slice

- The form is generated from `GET /api/metadata` `filterFieldRegistry` — one control per entry, in
  array order. Never hard-code a filter field, label, or option list in a component.
- `buildQueryDefinition` (pure) is the single place the form → `QueryDefinition` mapping lives; unit
  test it there.
- Paging and sorting are **server-side**: `ResultsTable` translates antd `Table.onChange` into
  `QueryDefinition.paging` / `.sort` and refetches; `SearchResponse.page.totalRows` drives the pager.
- `questionText` always comes from the server (`POST /api/search`) — no client-side Hebrew renderer.

## The saved-queries slice (S5)

- `features/saved-queries/`: `useSavedQueries` (list query + `rename` / `remove` / `run` mutations,
  all invalidating `['saved-queries']`) for the screen; `useCreateSavedQuery` (mutation only, no
  query) so `SaveQueryButton` on the search screen doesn't also fetch the list. `SavedQueriesTable`
  (per-row re-run / rename / delete), `RenameQueryModal` (parent keys it by query id — no sync
  effect), `SaveQueryButton` saves `form.definition`.
- `savedQueriesApi` is the only HTTP caller. Re-run returns a `SearchResponse`; `summarizeRun`
  turns it into `{ records, groups }` — records is the `count` metric summed over the returned
  groups (all groups fit one page in this PoC), groups is `page.totalRows`. Note the search engine
  aggregates: no `segmentation` ⇒ one group ⇒ `lastRunRowCount` is a group count, not a record
  count.
- Scope errors are 404s surfaced by `http.ts` like any other `ApiError` — no special handling.

## The NL slice (S6)

- `features/nl-query/`: `useNlParse` (a **mutation** — the user asks explicitly, one question at
  a time), `InterpretationPanel` (the server's read-back sentence + the filters field by field +
  what could not be mapped + the Run button), `NlQueryPage` wiring them together.
- **Parsing never runs a search.** `useSearch` now takes `QueryDefinition | undefined` and uses
  TanStack `skipToken` — the NL screen holds it `undefined` until the user clicks Run, then the
  normal `POST /api/search` path takes over (same `ResultsPanel`, paging and sorting). No second
  search implementation, no chat UI.
- `describeDefinition` (pure, unit-tested) maps a `QueryDefinition` to label/value rows using the
  same registry + reference labels the form is built from. It **reads** a definition; it does not
  compose Hebrew prose — the sentence is always `interpretationText` from the server.
- `nlQueryApi` is the only HTTP caller. Parse failures surface through `http.ts` like any other
  `ApiError`.
