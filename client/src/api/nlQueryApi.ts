import { http } from './http';
import type { NlParseRequest, NlParseResponse } from '../models/nlQuery';

export const nlQueryApi = {
  parse: (body: NlParseRequest) => http.post<NlParseResponse>('/api/nl-queries/parse', body),
};
