import type { FilterFieldRegistryEntry } from '../../models/metadata';
import {
  DEFAULT_PAGE_SIZE,
  type FilterValue,
  type QueryDefinition,
  type SortSpec,
} from '../../models/queryDefinition';

// Raw form value for one field: selected codes, or a year from/to pair being edited.
export type FieldValue = string[] | YearInput;
export interface YearInput {
  from?: number;
  to?: number;
}

// Earliest year the form offers, and the open end of a "to"-only range.
export const MIN_YEAR = 2000;

export interface SearchFormState {
  values: Record<string, FieldValue>;
  // Registry ids the user picked in "הוספת גרף לפי" — one chart each. Not the table shape.
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

// The results table is always this 3-way breakdown, so its columns never change as the user
// works. The "הוספת גרף לפי" picker only chooses which of these fields also gets a chart.
export const TABLE_BREAKDOWN = ['supportDomain', 'district', 'supportYear'];

const DEFAULT_SORT: SortSpec[] = [
  { field: 'supportYear', direction: 'desc' },
  { field: 'sumAmountApproved', direction: 'desc' },
];

// Turn the form state into the canonical QueryDefinition. Empty controls are omitted; a year
// field with both ends set becomes a range, with one end a single year. Cross-field validity is
// the server's job.
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
    // Guarded against a registry that drops one of the ids.
    segmentation: TABLE_BREAKDOWN.filter((id) => segmentableIds.has(id)),
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

  // yearRange. "To" alone reads as "עד שנה X" (up to and including), so it becomes a range from
  // MIN_YEAR; "from" alone keeps its single-year meaning. The asymmetry is deliberate.
  const { from, to } = Array.isArray(raw) ? ({} as YearInput) : raw;
  if (from != null && to != null) return { type: 'range', from, to };
  if (from != null) return { type: 'single', value: from };
  if (to != null) return { type: 'range', from: MIN_YEAR, to };
  return undefined;
}
