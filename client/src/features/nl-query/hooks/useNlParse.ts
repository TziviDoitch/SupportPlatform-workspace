import { useMutation } from '@tanstack/react-query';
import { nlQueryApi } from '../../../api/nlQueryApi';
import type { NlParseRequest } from '../../../models/nlQuery';

export function useNlParse() {
  return useMutation({
    mutationFn: (body: NlParseRequest) => nlQueryApi.parse(body),
  });
}
