import type { SearchResponse } from '../../models/search';

export interface RunSummary {
  /** Matching support requests — the `count` metric summed over the returned groups. */
  records: number;
  /** Number of result groups; 1 when the query has no segmentation. */
  groups: number;
}

/**
 * Reads a re-run's response into the two numbers the saved-queries screen shows. Group counts in
 * this PoC stay far below one page, so summing the returned page is exact.
 */
export function summarizeRun(response: SearchResponse): RunSummary {
  return {
    records: response.aggregations.reduce((sum, agg) => sum + (agg.metrics.count ?? 0), 0),
    groups: response.page.totalRows,
  };
}
