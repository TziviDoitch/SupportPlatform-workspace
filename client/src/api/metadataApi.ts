import { http } from './http';
import type { MetadataResponse } from '../models/metadata';

export const metadataApi = {
  get: (tenantId: string) =>
    http.get<MetadataResponse>(`/api/metadata?tenantId=${encodeURIComponent(tenantId)}`),
};
