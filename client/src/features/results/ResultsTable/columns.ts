import type { TableProps } from 'antd';
import { formatCurrencyIls } from '../../../lib/format';
import { labelForCode, labelForField } from '../../../lib/labels';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
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
 * the registry, values resolved to their Hebrew reference label) followed by one per metric.
 * Sorting is server-side — `sorter: true` only marks the header; the active order comes from
 * `sort` (the primary key). Pass `sortable: false` for a read-only render (the saved-query re-run
 * panel), so the headers aren't clickable-but-inert.
 */
export function buildColumns(
  segmentation: string[],
  metrics: string[],
  registry: FilterFieldRegistryEntry[],
  references: References,
  sort: SortSpec[],
  options: { sortable?: boolean } = {},
): NonNullable<TableProps<ResultRow>['columns']> {
  const { sortable = true } = options;
  const primary = sort[0];
  const sortProps = (field: string) => {
    if (!sortable) return {};
    const order =
      primary?.field === field
        ? primary.direction === 'asc'
          ? ('ascend' as const)
          : ('descend' as const)
        : null;
    return { sorter: true, sortOrder: order };
  };

  const segColumns = segmentation.map((id) => {
    const entry = registry.find((e) => e.id === id);
    const refList = entry?.referenceList ? references[entry.referenceList] : undefined;
    return {
      title: labelForField(registry, id),
      dataIndex: id,
      key: id,
      render: refList ? (value: unknown) => labelForCode(refList, value as string | number) : undefined,
      ...sortProps(id),
    };
  });

  const metricColumns = metrics.map((m) => ({
    title: METRIC_LABELS[m] ?? m,
    dataIndex: m,
    key: m,
    // No physical `align` — the cells inherit `text-align: start`, i.e. right under RTL, so the
    // numbers line up with their headers instead of drifting to the far edge.
    render: METRIC_RENDER[m],
    ...sortProps(m),
  }));

  return [...segColumns, ...metricColumns];
}
