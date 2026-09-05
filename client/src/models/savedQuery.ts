import type { QueryDefinition } from './queryDefinition';

export interface SavedQuery {
  id: string;
  name: string;
  definition: QueryDefinition;
  ownerUsername: string;
  tenantId: string;
  createdAt: string;
  lastRunAt: string | null;
  lastRunRowCount: number | null;
}

export interface SaveSavedQueryRequest {
  name: string;
  definition: QueryDefinition;
}
