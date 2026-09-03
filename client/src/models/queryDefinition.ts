/** The single canonical query object (`docs/contracts/query-definition.md`). The search form builds it. */

export type YearRangeValue = { type: 'range'; from: number; to: number };
export type YearSingleValue = { type: 'single'; value: number };

/** A code list (IN semantics) or a year filter. */
export type FilterValue = string[] | YearRangeValue | YearSingleValue;

export interface Paging {
  pageSize: number;
  pageNumber: number;
}

export type SortDirection = 'asc' | 'desc';

export interface SortSpec {
  field: string;
  direction: SortDirection;
}

export interface QueryDefinition {
  tenantId: string;
  /** Keys are registry field ids. May be empty. */
  filters: Record<string, FilterValue>;
  segmentation: string[];
  metrics: string[];
  paging: Paging;
  sort: SortSpec[];
}

export const DEFAULT_PAGE_SIZE = 50;
