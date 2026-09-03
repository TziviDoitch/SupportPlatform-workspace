/** Saved-query records (`docs/contracts/api-contract.md` §5). */
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

/** Body of `POST` / `PUT /api/saved-queries`. */
export interface SaveSavedQueryRequest {
  name: string;
  definition: QueryDefinition;
}
