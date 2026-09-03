import { http } from './http';
import type { SavedQuery, SaveSavedQueryRequest } from '../models/savedQuery';
import type { SearchResponse } from '../models/search';

const base = '/api/saved-queries';

export const savedQueriesApi = {
  list: () => http.get<SavedQuery[]>(base),
  create: (body: SaveSavedQueryRequest) => http.post<SavedQuery>(base, body),
  update: (id: string, body: SaveSavedQueryRequest) => http.put<SavedQuery>(`${base}/${id}`, body),
  remove: (id: string) => http.del<void>(`${base}/${id}`),
  run: (id: string) => http.post<SearchResponse>(`${base}/${id}/run`),
};
