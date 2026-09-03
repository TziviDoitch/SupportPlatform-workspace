import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { searchApi } from '../../../api/searchApi';
import type { QueryDefinition } from '../../../models/queryDefinition';

/** Runs `POST /api/search` for the given definition, keeping prior rows visible while refetching. */
export function useSearch(definition: QueryDefinition) {
  return useQuery({
    queryKey: ['search', definition],
    queryFn: () => searchApi.run(definition),
    placeholderData: keepPreviousData,
  });
}
