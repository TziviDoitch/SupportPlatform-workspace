import type { QueryDefinition, SortSpec } from '../models/queryDefinition';

/** Canonical "patch paging onto a definition" — the page/size the user picked in the table. */
export function withPaging(
  definition: QueryDefinition,
  pageNumber: number,
  pageSize: number,
): QueryDefinition {
  return { ...definition, paging: { pageNumber, pageSize } };
}

/** Canonical "patch sort onto a definition". A sort change resets to the first page. */
export function withSort(definition: QueryDefinition, sort: SortSpec[]): QueryDefinition {
  return { ...definition, sort, paging: { ...definition.paging, pageNumber: 1 } };
}
