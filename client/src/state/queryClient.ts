import { QueryClient } from '@tanstack/react-query';
import { ApiError } from '../models/problemDetails';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // A real HTTP answer (4xx/5xx) won't change on a retry — surface it now. Only retry
      // once for a network/parse failure, where a second attempt can legitimately succeed.
      retry: (failureCount, error) => !(error instanceof ApiError) && failureCount < 1,
      refetchOnWindowFocus: false,
    },
  },
});
