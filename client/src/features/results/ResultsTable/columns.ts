import type { TableProps } from 'antd';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { SortSpec } from '../../../models/queryDefinition';
import type { ResultRow } from '../../../models/search';

const METRIC_LABELS: Record<string, string> = {
  count: 'כמות',
  sumAmountApproved: 'סכום מאושר',
};

/**
 * Columns are derived from the query, not hard-coded: one per segmentation field (labelled from
 * the registry) followed by one per metric. Sorting is server-side — `sorter: true` only marks the
 * header; the active order comes from `sort`.
 */
export function buildColumns(
  segmentation: string[],
  metrics: string[],
  registry: FilterFieldRegistryEntry[],
  sort: SortSpec[],
): NonNullable<TableProps<ResultRow>['columns']> {
  const labelFor = (id: string) => registry.find((e) => e.id === id)?.label ?? id;
  const orderFor = (field: string) => {
    const dir = sort.find((s) => s.field === field)?.direction;
    return dir === 'asc' ? ('ascend' as const) : dir === 'desc' ? ('descend' as const) : null;
  };

  const segColumns = segmentation.map((id) => ({
    title: labelFor(id),
    dataIndex: id,
    key: id,
    sorter: true,
    sortOrder: orderFor(id),
  }));

  const metricColumns = metrics.map((m) => ({
    title: METRIC_LABELS[m] ?? m,
    dataIndex: m,
    key: m,
    sorter: true,
    sortOrder: orderFor(m),
    align: 'left' as const,
  }));

  return [...segColumns, ...metricColumns];
}
