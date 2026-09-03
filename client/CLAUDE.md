# client — React + TypeScript (Vite)

Scope and stages: [`../IMPLEMENTATION_PLAN.md`](../IMPLEMENTATION_PLAN.md).
Component patterns + worked table example: the
[`react-components`](.claude/skills/react-components/SKILL.md) skill.

## Stack

React 19 + TypeScript + Vite 8 · Ant Design v6 + `@ant-design/icons` (`ConfigProvider direction="rtl"` + theme tokens) ·
react-router-dom v7 · TanStack Query v5 · Chart.js (`react-chartjs-2`, added in S7) ·
Vitest + `@testing-library/react` + jsdom · lint via **oxlint** (not ESLint).

antd v6 is CSS-in-JS — no stylesheet import. RTL comes from `<html dir="rtl">` in
`index.html` plus the `ConfigProvider`.

## Structure (plan §4)

```
src/
  main.tsx     entry — providers: QueryClientProvider > ConfigProvider(rtl, he_IL) > BrowserRouter > App
  App/         app shell — Layout + Menu nav + ErrorBoundary + <Routes>; routes.tsx is the route list (data)
  api/         http.ts (fetch wrapper + ProblemDetails→notification interceptor), config.ts, typed services
  components/  generic reusable components (DataTable, BarChart, PageLoader, ...)
  hooks/       cross-feature hooks (useMetadata) — a hook used by one feature lives under that feature
  lib/         pure, framework-free helpers: format.ts (he-IL currency/date), labels.ts (code/field → label), queryDefinition.ts (withPaging/withSort)
  features/
    search/  results/  saved-queries/  nl-query/
      each: components in <Component>/ folders, hooks in hooks/, feature-local helpers alongside
  state/       queryClient.ts — the shared TanStack Query client only (per-feature hooks live under the feature)
  models/      shared TypeScript types (metadata, queryDefinition, search, problemDetails)
test/setup.ts  Vitest setup — RTL cleanup + ResizeObserver/matchMedia stubs for antd
```

Search results are shown **inline on `SearchPage`** — there is no `/results` route (removed in S3;
S7 kept it inline and added the chart to `ResultsPanel` instead). The app theme (antd tokens) lives
in `src/theme.ts` and is applied once via `ConfigProvider` in `main.tsx`; cards are borderless with a
faint dark-purple `boxShadowTertiary`, and `components/SectionTitle` renders the dark-purple leading
icon used by every card title and page heading. `features/results/` holds the results components
(`ResultsSection`, `ResultsPanel`, `ResultsTable`, `ResultsChart`, `QuestionPanel`), `useSearch`, and
the pure `buildCharts` (`buildChartData.ts`). **Every screen that shows search results renders
`ResultsSection`** (search / nl-query / saved-queries re-run) so they look identical — question panel,
a spinner until the first response, then the table with a chart per graph field.

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
- No `any` — `tsconfig` has `strict: true`, so this is compiler-enforced.
- Derive state, don't duplicate it.
- Every data view handles loading / empty / error; `App/ErrorBoundary` is the last-resort catch for
  a thrown render. Page-level loading uses `components/PageLoader` (centered), not a bare `<Spin/>`.

## API seam (`src/api/`)

- `http.get` / `http.post` / `http.put` / `http.del` are the only callers of `fetch`. On a non-2xx
  response `http.ts` parses RFC 7807 ProblemDetails, raises one error toast (title + `detail` +
  `traceId`, formatted by `formatProblemDetail` in `models/problemDetails`), and throws `ApiError` —
  hooks/components surface it from that, they don't re-notify.
- **One surface per failure.** A call whose screen shows the error inline passes `{ notify: false }`
  as the last arg so the toast is suppressed (`searchApi.run` does this — `ResultsPanel` renders the
  banner). Mutations and parse keep the toast (they have no inline surface).
- Toasts go through `api/notificationHost` (`notifyError` / `notifySuccess`), never a direct
  `notification.*` call. `App/NotificationBridge` (rendered inside antd `<App>` in `main.tsx`)
  registers the context-aware `notification` instance so toasts inherit theme + RTL; `notificationHost`
  falls back to antd's static API when nothing is registered (unit tests). Don't `import { notification }`
  anywhere outside `notificationHost`.
- `queryClient` retries only network/parse failures once; a real HTTP answer (`ApiError`) is surfaced
  immediately, not retried.
- Every request carries an `X-User` header (`DEFAULT_USER`, `api/config.ts`) — the PoC identity seam
  (the server derives tenant + role from it and 403s on a body `tenantId` mismatch). `http.post` body
  is optional (for `POST .../run`).
- One service per resource (`metadataApi`, `searchApi`, `savedQueriesApi`, `nlQueryApi`), each
  returning a `models/` type.
- `DEFAULT_TENANT_ID` / `DEFAULT_USER` (`api/config.ts`) are fixed stand-ins until a real login flow
  lands — do not scatter tenant/user literals elsewhere.

## The search slice

- The form is generated from `GET /api/metadata` `filterFieldRegistry` — one control per entry, in
  array order. Never hard-code a filter field, label, or option list in a component.
- `buildQueryDefinition` (pure) is the single place the form → `QueryDefinition` mapping lives; unit
  test it there.
- The search runs on an explicit **"חיפוש"** click, not on every keystroke. `SearchPage` keeps the
  last-run definition in `submitted` state; `SearchForm`'s "ניקוי מאפייני חיפוש" clears the form
  (`useSearchForm.reset`). The filter panel is collapsible (local state in `SearchForm`).
- Paging and sorting are **server-side**: `ResultsTable` translates antd `Table.onChange` into a
  `withPaging` / `withSort` patch on `submitted`; `SearchResponse.page.totalRows` drives the pager.
- `questionText` always comes from the server (`POST /api/search`) — no client-side Hebrew renderer.
- The segmentation control is labelled **"הוספת גרף לפי"**: each field it adds gets a bucket column
  in the table **and** a bar chart. `ResultsPanel` lays the table first, then one `ResultsChart` per
  field **beside** it (antd `Row`/`Col`, wraps; stacks on narrow) — it never falls back to a
  table-only view. `buildCharts` (pure, `buildChartData.ts`) is the single aggregations → per-field
  `ChartData[]` mapping: for each field it sums `count` per bucket, marginalising over the other
  fields. Unit-test chart logic there, not in the component. `components/BarChart` is a generic
  `react-chartjs-2` wrapper (registers `chart.js` once).
- `YearRangeField` is two year `Select`s (2000 – next year), "to" hiding years before "from" — no
  free-form number inputs. The server still validates the range.

## The saved-queries slice (S5)

- `features/saved-queries/`: `useSavedQueries` (list query + `rename` / `remove` / `run` mutations,
  all invalidating `['saved-queries']`) for the screen; `useCreateSavedQuery` (mutation only, no
  query) so `SaveQueryButton` on the search screen doesn't also fetch the list. `SavedQueriesTable`
  (per-row re-run / rename / delete), `RenameQueryModal` (parent keys it by query id — no sync
  effect), `SaveQueryButton` saves `form.definition`.
- `savedQueriesApi` is the only HTTP caller. Re-run returns a `SearchResponse`; the screen renders
  the **same `ResultsSection`** the search screen uses (question + chart(s) + table), read-only —
  `POST /{id}/run` takes no definition override, so no paging/sort; adjust a query on the search
  screen. `ResultsTable`'s header shows the record count and, when `sumAmountApproved` is a metric,
  the total approved (summed over `aggregations`; all groups fit one page in this PoC).
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
