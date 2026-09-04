import { useMemo } from 'react';
import { Card, Typography, type TableProps } from 'antd';
import { TableOutlined } from '@ant-design/icons';
import { DataTable } from '../../../components/DataTable';
import { SectionTitle } from '../../../components/SectionTitle';
import { formatCurrencyIls, formatIntHe } from '../../../lib/format';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import { DEFAULT_PAGE_SIZE, type QueryDefinition, type SortSpec } from '../../../models/queryDefinition';
import type { ResultRow, SearchResponse } from '../../../models/search';
import { buildColumns } from './columns';

interface Props {
  response: SearchResponse | undefined;
  registry: FilterFieldRegistryEntry[];
  references: References;
  definition: QueryDefinition;
  loading?: boolean;
  /** Omit both for a read-only table: no pager, non-sortable headers. */
  onPageChange?: (pageNumber: number, pageSize: number) => void;
  onSortChange?: (sort: SortSpec[]) => void;
}

/** Thin wiring: dynamic columns + server-side paging/sort over the generic {@link DataTable}. */
export function ResultsTable({
  response,
  registry,
  references,
  definition,
  loading,
  onPageChange,
  onSortChange,
}: Props) {
  const interactive = onPageChange !== undefined || onSortChange !== undefined;

  const columns = useMemo(
    () =>
      buildColumns(definition.segmentation, definition.metrics, registry, references, definition.sort, {
        sortable: interactive,
      }),
    [definition.segmentation, definition.metrics, registry, references, definition.sort, interactive],
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
      onSortChange?.(nextSort);
    } else {
      onPageChange?.(pagination.current ?? 1, pagination.pageSize ?? DEFAULT_PAGE_SIZE);
    }
  };

  const totalRows = response?.page.totalRows ?? 0;
  const approved =
    response && definition.metrics.includes('sumAmountApproved')
      ? response.aggregations.reduce((sum, agg) => sum + (agg.metrics.sumAmountApproved ?? 0), 0)
      : undefined;

  return (
    <Card
      title={<SectionTitle icon={<TableOutlined />}>תוצאות</SectionTitle>}
      extra={
        response ? (
          <Typography.Text type="secondary">
            סה״כ {formatIntHe(totalRows)} רשומות
            {approved !== undefined && ` · סכום מאושר ${formatCurrencyIls(approved)}`}
          </Typography.Text>
        ) : null
      }
      style={{ height: '100%' }}
    >
      <DataTable<ResultRow>
        columns={columns}
        rows={response?.rows ?? []}
        rowKey={rowKey}
        loading={loading}
        onChange={interactive ? handleChange : undefined}
        pagination={
          interactive
            ? {
                current: response?.page.pageNumber ?? 1,
                pageSize: response?.page.pageSize ?? DEFAULT_PAGE_SIZE,
                total: totalRows,
                showSizeChanger: false,
              }
            : false
        }
      />
    </Card>
  );
}
