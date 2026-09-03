import { labelForCode } from '../../lib/labels';
import type { FilterFieldRegistryEntry, References } from '../../models/metadata';
import type { AggregationRow } from '../../models/search';

export interface ChartData {
  labels: string[];
  values: number[];
  /** Registry label of the segmentation field this chart is for — the card title + dataset label. */
  seriesLabel: string;
}

/** The metric the bar charts plot. Fixed to `count` in this PoC. */
const CHART_METRIC = 'count';

/**
 * One bar chart per segmentation field: the marginal distribution of `count` over that field's
 * buckets, summed across the other segmentation fields. Returns `[]` when there is nothing to plot
 * (no groups, no segmentation, or none of the fields are in the registry).
 *
 * Summing the returned `aggregations` is exact because the server's groups partition the data and
 * `count` is additive — and in this PoC every group fits on the first page (see `summarizeRun`).
 * Bucket labels resolve through the same reference lists the form is built from; a `yearRange`
 * field has no list, so its numeric key shows as-is.
 */
export function buildCharts(
  aggregations: AggregationRow[],
  segmentation: string[],
  registry: FilterFieldRegistryEntry[],
  references: References,
): ChartData[] {
  if (aggregations.length === 0) return [];

  const charts: ChartData[] = [];

  for (const fieldId of segmentation) {
    const entry = registry.find((e) => e.id === fieldId);
    if (!entry) continue;

    const refList = entry.referenceList ? references[entry.referenceList] : undefined;

    // Sum the metric per bucket, keyed by the raw code so distinct codes never merge.
    const totals = new Map<string, { label: string; value: number }>();
    for (const agg of aggregations) {
      const raw = agg.key[fieldId];
      if (raw === undefined) continue;
      const code = String(raw);
      const current = totals.get(code);
      const add = Number(agg.metrics[CHART_METRIC] ?? 0);
      if (current) current.value += add;
      else totals.set(code, { label: labelForCode(refList, raw), value: add });
    }

    if (totals.size === 0) continue;

    const buckets = [...totals.values()];
    charts.push({
      labels: buckets.map((b) => b.label),
      values: buckets.map((b) => b.value),
      seriesLabel: entry.label,
    });
  }

  return charts;
}
