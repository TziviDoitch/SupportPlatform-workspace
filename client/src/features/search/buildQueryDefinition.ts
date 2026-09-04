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

/** Earliest year the form offers, and the open end of a "to"-only range. */
export const MIN_YEAR = 2000;

export interface SearchFormState {
  /** Keyed by registry field id. */
  values: Record<string, FieldValue>;
  /** Registry ids the user picked in "הוספת גרף לפי" — one chart each. Not the table shape. */
  graphFields: string[];
  pageNumber: number;
  pageSize: number;
  sort: SortSpec[];
}

export const emptyFormState: SearchFormState = {
  values: {},
  graphFields: [],
  pageNumber: 1,
  pageSize: DEFAULT_PAGE_SIZE,
  sort: [],
};

/**
 * The results table is always this 3-way breakdown, so its columns never change as the user works.
 * The "הוספת גרף לפי" picker only chooses which of these fields also gets a chart. These are the
 * `segmentable` registry ids we break out (body type stays a filter, not a breakdown dimension).
 */
export const TABLE_BREAKDOWN = ['supportDomain', 'district', 'supportYear'];

/** Default ordering when the user hasn't clicked a column header: newest year first, then largest sum. */
const DEFAULT_SORT: SortSpec[] = [
  { field: 'supportYear', direction: 'desc' },
  { field: 'sumAmountApproved', direction: 'desc' },
];

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
    // Fixed breakdown → a stable table. Guarded against a registry that drops one of the ids.
    segmentation: TABLE_BREAKDOWN.filter((id) => segmentableIds.has(id)),
    // Both contract metrics — count answers the question, sumAmountApproved gives the table a
    // second real column. The server always computes both (contract §3).
    metrics: ['count', 'sumAmountApproved'],
    paging: { pageNumber: state.pageNumber, pageSize: state.pageSize },
    sort: state.sort.length > 0 ? state.sort : DEFAULT_SORT,
  };
}

function toFilterValue(entry: FilterFieldRegistryEntry, raw: FieldValue): FilterValue | undefined {
  if (entry.kind === 'codeList') {
    const codes = Array.isArray(raw) ? raw : [];
    return codes.length > 0 ? codes : undefined;
  }

  // yearRange. "To" alone reads as "עד שנה X" — up to and including — so it becomes a range from
  // MIN_YEAR, not an equality filter on that single year. "From" alone keeps its existing
  // single-year meaning (ARCHITECTURE §6.1); the asymmetry is deliberate and untouched here.
  const { from, to } = Array.isArray(raw) ? ({} as YearInput) : raw;
  if (from != null && to != null) return { type: 'range', from, to };
  if (from != null) return { type: 'single', value: from };
  if (to != null) return { type: 'range', from: MIN_YEAR, to };
  return undefined;
}
