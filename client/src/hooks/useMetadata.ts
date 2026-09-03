import { useQuery } from '@tanstack/react-query';
import { metadataApi } from '../api/metadataApi';

/** Loads the reference lists + filter-field registry that the search form is built from. */
export function useMetadata(tenantId: string) {
  return useQuery({
    queryKey: ['metadata', tenantId],
    queryFn: () => metadataApi.get(tenantId),
    staleTime: Infinity,
  });
}
