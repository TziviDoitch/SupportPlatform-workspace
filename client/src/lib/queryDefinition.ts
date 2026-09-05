import type { QueryDefinition, SortSpec } from '../models/queryDefinition';

export function withPaging(
  definition: QueryDefinition,
  pageNumber: number,
  pageSize: number,
): QueryDefinition {
  return { ...definition, paging: { pageNumber, pageSize } };
}

export function withSort(definition: QueryDefinition, sort: SortSpec[]): QueryDefinition {
  return { ...definition, sort, paging: { ...definition.paging, pageNumber: 1 } };
}
