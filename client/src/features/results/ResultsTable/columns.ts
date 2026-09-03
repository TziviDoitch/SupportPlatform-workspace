import type { TableProps } from 'antd';
import { formatCurrencyIls } from '../../../lib/format';
import { labelForField } from '../../../lib/labels';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { SortSpec } from '../../../models/queryDefinition';
import type { ResultRow } from '../../../models/search';

const METRIC_LABELS: Record<string, string> = {
  count: 'כמות',
  sumAmountApproved: 'סכום מאושר',
};

const METRIC_RENDER: Record<string, (value: unknown) => string> = {
  sumAmountApproved: (value) => formatCurrencyIls(Number(value) || 0),
};

/**
 * Columns are derived from the query, not hard-coded: one per segmentation field (labelled from
 * the registry) followed by one per metric. Sorting is server-side — `sorter: true` only marks the
 * header; the active order comes from `sort`. Pass `sortable: false` for a read-only render (the
 * saved-query re-run panel), so the headers aren't clickable-but-inert.
 */
export function buildColumns(
  segmentation: string[],
  metrics: string[],
  registry: FilterFieldRegistryEntry[],
  sort: SortSpec[],
  options: { sortable?: boolean } = {},
): NonNullable<TableProps<ResultRow>['columns']> {
  const { sortable = true } = options;
  const labelFor = (id: string) => labelForField(registry, id);
  const sortProps = (field: string) => {
    if (!sortable) return {};
    const dir = sort.find((s) => s.field === field)?.direction;
    const order = dir === 'asc' ? ('ascend' as const) : dir === 'desc' ? ('descend' as const) : null;
    return { sorter: true, sortOrder: order };
  };

  const segColumns = segmentation.map((id) => ({
    title: labelFor(id),
    dataIndex: id,
    key: id,
    ...sortProps(id),
  }));

  const metricColumns = metrics.map((m) => ({
    title: METRIC_LABELS[m] ?? m,
    dataIndex: m,
    key: m,
    align: 'left' as const,
    render: METRIC_RENDER[m],
    ...sortProps(m),
  }));

  return [...segColumns, ...metricColumns];
}
