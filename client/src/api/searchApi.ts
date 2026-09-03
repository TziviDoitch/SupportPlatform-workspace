import { http } from './http';
import type { QueryDefinition } from '../models/queryDefinition';
import type { SearchResponse } from '../models/search';

export const searchApi = {
  run: (definition: QueryDefinition) => http.post<SearchResponse>('/api/search', definition),
};
