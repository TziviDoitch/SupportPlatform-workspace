export type YearRangeValue = { type: 'range'; from: number; to: number };
export type YearSingleValue = { type: 'single'; value: number };

export type FilterValue = string[] | YearRangeValue | YearSingleValue;

export interface Paging {
  pageSize: number;
  pageNumber: number;
}

export type SortDirection = 'asc' | 'desc';

export type MetricName = 'count' | 'sumAmountApproved';

export interface SortSpec {
  field: string;
  direction: SortDirection;
}

export interface QueryDefinition {
  tenantId: string;
  filters: Record<string, FilterValue>;
  segmentation: string[];
  metrics: MetricName[];
  paging: Paging;
  sort: SortSpec[];
}

export const DEFAULT_PAGE_SIZE = 50;
