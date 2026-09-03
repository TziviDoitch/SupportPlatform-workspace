import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { savedQueriesApi } from '../../../api/savedQueriesApi';
import type { SaveSavedQueryRequest } from '../../../models/savedQuery';

const QUERY_KEY = ['saved-queries'];

/** Create a saved query and refresh the list. Standalone so the search screen can save
 *  without also subscribing to the list query. */
export function useCreateSavedQuery() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: SaveSavedQueryRequest) => savedQueriesApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEY }),
  });
}

/** List + rename / delete / re-run for the saved-queries screen. Mutations refresh the list. */
export function useSavedQueries() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: QUERY_KEY });

  const list = useQuery({ queryKey: QUERY_KEY, queryFn: savedQueriesApi.list });
  const create = useCreateSavedQuery();

  const rename = useMutation({
    mutationFn: ({ id, body }: { id: string; body: SaveSavedQueryRequest }) =>
      savedQueriesApi.update(id, body),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id: string) => savedQueriesApi.remove(id),
    onSuccess: invalidate,
  });

  const run = useMutation({
    mutationFn: (id: string) => savedQueriesApi.run(id),
    onSuccess: invalidate,
  });

  return { list, create, rename, remove, run };
}
