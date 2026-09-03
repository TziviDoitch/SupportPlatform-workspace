import { useMemo } from 'react';
import type { TableProps } from 'antd';
import { DataTable } from '../../../components/DataTable';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import { DEFAULT_PAGE_SIZE, type QueryDefinition, type SortSpec } from '../../../models/queryDefinition';
import type { ResultRow, SearchResponse } from '../../../models/search';
import { buildColumns } from './columns';

interface Props {
  response: SearchResponse | undefined;
  registry: FilterFieldRegistryEntry[];
  definition: QueryDefinition;
  loading?: boolean;
  onPageChange: (pageNumber: number, pageSize: number) => void;
  onSortChange: (sort: SortSpec[]) => void;
}

/** Thin wiring: dynamic columns + server-side paging/sort over the generic {@link DataTable}. */
export function ResultsTable({
  response,
  registry,
  definition,
  loading,
  onPageChange,
  onSortChange,
}: Props) {
  const columns = useMemo(
    () => buildColumns(definition.segmentation, definition.metrics, registry, definition.sort),
    [definition.segmentation, definition.metrics, definition.sort, registry],
  );

  // Result rows have no id — key them by their segmentation values (unique per bucket on a page);
  // an unsegmented query has a single total row.
  const rowKey = (row: ResultRow) =>
    definition.segmentation.length > 0
      ? definition.segmentation.map((id) => row[id]).join('|')
      : 'total';

  const handleChange: TableProps<ResultRow>['onChange'] = (pagination, _filters, sorter) => {
    const active = Array.isArray(sorter) ? sorter[0] : sorter;
    const nextSort: SortSpec[] =
      active && active.order && active.columnKey != null
        ? [{ field: String(active.columnKey), direction: active.order === 'ascend' ? 'asc' : 'desc' }]
        : [];

    if (JSON.stringify(nextSort) !== JSON.stringify(definition.sort)) {
      onSortChange(nextSort);
    } else {
      onPageChange(pagination.current ?? 1, pagination.pageSize ?? DEFAULT_PAGE_SIZE);
    }
  };

  return (
    <DataTable<ResultRow>
      columns={columns}
      rows={response?.rows ?? []}
      rowKey={rowKey}
      loading={loading}
      onChange={handleChange}
      pagination={{
        current: response?.page.pageNumber ?? 1,
        pageSize: response?.page.pageSize ?? DEFAULT_PAGE_SIZE,
        total: response?.page.totalRows ?? 0,
        showSizeChanger: false,
      }}
    />
  );
}
