import type { SearchResponse } from '../../models/search';

export interface RunSummary {
  /** Matching support requests — the `count` metric summed over the returned groups. */
  records: number;
  /** Total approved amount (ILS) — `sumAmountApproved` summed over the returned groups. */
  approved: number;
  /** Number of result groups; 1 when the query has no segmentation. */
  groups: number;
}

/**
 * Reads a re-run's response into the numbers the saved-queries screen shows. Group counts in this
 * PoC stay far below one page, so summing the returned page is exact.
 */
export function summarizeRun(response: SearchResponse): RunSummary {
  const sum = (metric: string) =>
    response.aggregations.reduce((total, agg) => total + (agg.metrics[metric] ?? 0), 0);

  return {
    records: sum('count'),
    approved: sum('sumAmountApproved'),
    groups: response.page.totalRows,
  };
}
