import { http } from './http';
import type { QueryDefinition } from '../models/queryDefinition';
import type { SearchResponse } from '../models/search';

export const searchApi = {
  // `notify: false` — a failed search is shown inline by `ResultsPanel`, not as a toast.
  run: (definition: QueryDefinition) =>
    http.post<SearchResponse>('/api/search', definition, { notify: false }),
};
