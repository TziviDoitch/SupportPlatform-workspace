import { useQuery } from '@tanstack/react-query';
import { metadataApi } from '../api/metadataApi';

export function useMetadata(tenantId: string) {
  return useQuery({
    queryKey: ['metadata', tenantId],
    queryFn: () => metadataApi.get(tenantId),
    staleTime: Infinity,
  });
}
