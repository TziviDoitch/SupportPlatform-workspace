import type { FilterFieldRegistryEntry } from '../../models/metadata';
import {
  DEFAULT_PAGE_SIZE,
  type FilterValue,
  type QueryDefinition,
  type SortSpec,
} from '../../models/queryDefinition';

/** Raw form value for one field: selected codes, or a year from/to pair being edited. */
export type FieldValue = string[] | YearInput;
export interface YearInput {
  from?: number;
  to?: number;
}

export interface SearchFormState {
  /** Keyed by registry field id. */
  values: Record<string, FieldValue>;
  segmentation: string[];
  pageNumber: number;
  pageSize: number;
  sort: SortSpec[];
}

export const emptyFormState: SearchFormState = {
  values: {},
  segmentation: [],
  pageNumber: 1,
  pageSize: DEFAULT_PAGE_SIZE,
  sort: [],
};

/**
 * Turn the form state into the canonical {@link QueryDefinition}. Empty controls are omitted;
 * a year field with both ends set becomes a range, with one end set a single year. Cross-field
 * validity (reversed range, unknown ids) is the server's job — see `docs/contracts`.
 */
export function buildQueryDefinition(
  state: SearchFormState,
  registry: FilterFieldRegistryEntry[],
  tenantId: string,
): QueryDefinition {
  const filters: Record<string, FilterValue> = {};

  for (const entry of registry) {
    const raw = state.values[entry.id];
    const value = raw === undefined ? undefined : toFilterValue(entry, raw);
    if (value !== undefined) {
      filters[entry.id] = value;
    }
  }

  const segmentableIds = new Set(registry.filter((e) => e.segmentable).map((e) => e.id));

  return {
    tenantId,
    filters,
    segmentation: state.segmentation.filter((id) => segmentableIds.has(id)),
    // Both contract metrics — count answers the question, sumAmountApproved gives the table a
    // second real column. The server always computes both (contract §3).
    metrics: ['count', 'sumAmountApproved'],
    paging: { pageNumber: state.pageNumber, pageSize: state.pageSize },
    sort: state.sort,
  };
}

function toFilterValue(entry: FilterFieldRegistryEntry, raw: FieldValue): FilterValue | undefined {
  if (entry.kind === 'codeList') {
    const codes = Array.isArray(raw) ? raw : [];
    return codes.length > 0 ? codes : undefined;
  }

  // yearRange
  const { from, to } = Array.isArray(raw) ? ({} as YearInput) : raw;
  if (from != null && to != null) return { type: 'range', from, to };
  if (from != null) return { type: 'single', value: from };
  if (to != null) return { type: 'single', value: to };
  return undefined;
}
