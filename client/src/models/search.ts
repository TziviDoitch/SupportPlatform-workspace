export type ResultRow = Record<string, string | number>;

export interface AggregationRow {
  key: Record<string, string | number>;
  metrics: Record<string, number>;
}

export interface PageInfo {
  pageNumber: number;
  pageSize: number;
  totalGroups: number;
}

export interface ExecutionMeta {
  durationMs: number;
  rowCount: number;
  cacheHit: boolean;
  definitionHash: string;
}

export interface SearchResponse {
  questionText: string;
  rows: ResultRow[];
  aggregations: AggregationRow[];
  page: PageInfo;
  executionMeta: ExecutionMeta;
}
