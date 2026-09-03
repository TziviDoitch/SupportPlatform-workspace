import type { FilterFieldRegistryEntry, References } from '../../models/metadata';
import type { AggregationRow } from '../../models/search';

export interface ChartData {
  labels: string[];
  values: number[];
  /** Registry label of the single segmentation field — the dataset / axis title. */
  seriesLabel: string;
}

/** The metric the bar chart plots. Fixed to `count` in this PoC (one static chart, §7). */
const CHART_METRIC = 'count';

/**
 * Maps a search response's `aggregations` to bar-chart series. Returns `null` unless the query has
 * exactly one segmentation field and at least one group — the only shape a single bar chart can
 * show. Bucket labels are resolved through the same reference lists the form is built from; a
 * `yearRange` field has no reference list, so its numeric key is shown as-is.
 */
export function buildChartData(
  aggregations: AggregationRow[],
  segmentation: string[],
  registry: FilterFieldRegistryEntry[],
  references: References,
): ChartData | null {
  if (segmentation.length !== 1 || aggregations.length === 0) return null;

  const fieldId = segmentation[0];
  const entry = registry.find((e) => e.id === fieldId);
  if (!entry) return null;

  const refList = entry.referenceList ? references[entry.referenceList] : undefined;
  const labelFor = (code: string | number): string =>
    refList?.find((r) => r.code === String(code))?.label ?? String(code);

  const labels: string[] = [];
  const values: number[] = [];
  for (const agg of aggregations) {
    labels.push(labelFor(agg.key[fieldId]));
    values.push(Number(agg.metrics[CHART_METRIC] ?? 0));
  }

  return { labels, values, seriesLabel: entry.label };
}
