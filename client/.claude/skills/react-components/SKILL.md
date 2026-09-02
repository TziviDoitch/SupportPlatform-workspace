---
name: react-components
description: How to build and refactor React components in the SupportPlatform client — small components, one per folder, UI/logic split, typed data services. Apply when creating or changing any component, hook, or feature screen under client/src. בעברית: כתיבת רכיבי React בצד הלקוח.
---

# React Components — client

## Rules

- **Small components, one per folder**: `Name/Name.tsx` + `Name/index.ts` (re-export).
- **UI vs logic**: components render; hooks (`useX.ts`) hold state, effects, mapping, business logic.
  No calculations or data shaping inside JSX.
- **Data services**: all HTTP in `src/api/*.ts`, returning typed results. Components and hooks call
  services, never `fetch`/`axios` directly.
- **Types** in `src/models/` (or co-located `*.types.ts`). Never inline object shapes.
- **Placement**: generic reusable component → `src/components/`. Feature-only component → under that
  feature (`src/features/<feature>/<Component>/`).
- No `any`. Memoize column/config arrays. Handle loading / empty / error on every data view.

## Worked example: extract a table

### Before — everything in one JSX file

```tsx
// features/results/ResultsPage.tsx  — columns, paging, mapping, fetch all mixed in
function ResultsPage() {
  const { data, isLoading } = useQuery(['search'], () => fetch('/api/search').then(r => r.json()));
  const rows = (data?.rows ?? []).map((r: any) => ({ ...r, year: r.supportYear }));
  return (
    <Table
      loading={isLoading}
      dataSource={rows}
      rowKey="id"
      pagination={{ pageSize: 50 }}
      columns={[
        { title: 'Body', dataIndex: 'submittingBodyName' },
        { title: 'Domain', dataIndex: 'supportDomain' },
        { title: 'Year', dataIndex: 'year' },
        { title: 'Status', dataIndex: 'status' },
      ]}
    />
  );
}
```

### After — one job per file

```tsx
// src/components/DataTable/DataTable.tsx  — generic, reusable, no domain knowledge
import { Table } from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';

interface DataTableProps<T> {
  columns: ColumnsType<T>;
  rows: T[];
  rowKey: keyof T & string;
  loading?: boolean;
  pagination?: TablePaginationConfig | false;
}

export function DataTable<T extends object>({
  columns, rows, rowKey, loading, pagination,
}: DataTableProps<T>) {
  return (
    <Table<T>
      columns={columns}
      dataSource={rows}
      rowKey={rowKey}
      loading={loading}
      pagination={pagination ?? { pageSize: 50 }}
      size="middle"
    />
  );
}
```

```ts
// src/components/DataTable/index.ts
export { DataTable } from './DataTable';
```

```ts
// src/models/search.ts  — the typed boundary
export interface SupportRequestRow {
  id: string;
  submittingBodyName: string;
  supportDomain: string;
  supportYear: number;
  status: string;
}
```

```ts
// src/api/searchApi.ts  — the only place that talks HTTP
import { http } from './http';
import type { SupportRequestRow } from '../models/search';

export const searchApi = {
  run: (definition: unknown) =>
    http.post<{ rows: SupportRequestRow[] }>('/api/search', definition),
};
```

```ts
// src/features/results/hooks/useSearchResults.ts  — logic, no JSX
import { useQuery } from '@tanstack/react-query';
import { searchApi } from '../../../api/searchApi';
import type { QueryDefinition } from '../../../models/queryDefinition';

export function useSearchResults(definition: QueryDefinition) {
  const query = useQuery({
    queryKey: ['search', definition],
    queryFn: () => searchApi.run(definition),
  });
  return { rows: query.data?.rows ?? [], isLoading: query.isLoading, error: query.error };
}
```

```tsx
// src/features/results/ResultsTable/columns.tsx
import type { ColumnsType } from 'antd/es/table';
import type { SupportRequestRow } from '../../../models/search';

export const resultColumns: ColumnsType<SupportRequestRow> = [
  { title: 'Body', dataIndex: 'submittingBodyName', key: 'submittingBodyName' },
  { title: 'Domain', dataIndex: 'supportDomain', key: 'supportDomain' },
  { title: 'Year', dataIndex: 'supportYear', key: 'supportYear' },
  { title: 'Status', dataIndex: 'status', key: 'status' },
];
```

```tsx
// src/features/results/ResultsTable/ResultsTable.tsx  — thin, just wiring
import { DataTable } from '../../../components/DataTable';
import type { SupportRequestRow } from '../../../models/search';
import { resultColumns } from './columns';

interface Props { rows: SupportRequestRow[]; loading?: boolean; }

export function ResultsTable({ rows, loading }: Props) {
  return <DataTable<SupportRequestRow> rows={rows} columns={resultColumns} rowKey="id" loading={loading} />;
}
```

### Why

- `DataTable` — reuse across every feature; styling/paging decided once.
- `columns.tsx` — data, not logic; easy to test and to change per feature.
- `useSearchResults` — logic is unit-testable without rendering.
- `searchApi` + `models/` — one typed seam to the server; swap transport without touching UI.
- `ResultsTable` — pure wiring, trivial to read.
