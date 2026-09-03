import { keepPreviousData, skipToken, useQuery } from '@tanstack/react-query';
import { searchApi } from '../../../api/searchApi';
import type { QueryDefinition } from '../../../models/queryDefinition';

/**
 * Runs `POST /api/search` for the given definition, keeping prior rows visible while refetching.
 * `undefined` means "nothing to run yet" — the NL screen holds it that way until the user has
 * reviewed the interpretation and pressed Run.
 */
export function useSearch(definition: QueryDefinition | undefined) {
  return useQuery({
    queryKey: ['search', definition],
    queryFn: definition ? () => searchApi.run(definition) : skipToken,
    placeholderData: keepPreviousData,
  });
}
