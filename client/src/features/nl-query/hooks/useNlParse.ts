import { useMutation } from '@tanstack/react-query';
import { nlQueryApi } from '../../../api/nlQueryApi';
import type { NlParseRequest } from '../../../models/nlQuery';

/** Parses a free-text question. A mutation, not a query: the user asks explicitly, one question at a time. */
export function useNlParse() {
  return useMutation({
    mutationFn: (body: NlParseRequest) => nlQueryApi.parse(body),
  });
}
