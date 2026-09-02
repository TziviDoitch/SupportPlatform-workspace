import { QueryClient } from '@tanstack/react-query';

/** Shared TanStack Query client. Per-feature hooks live under each feature's folder. */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});
