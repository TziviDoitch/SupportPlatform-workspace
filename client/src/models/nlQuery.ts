/** Free text → `QueryDefinition` (`docs/contracts/api-contract.md` §4). Parsing does not run the query. */

import type { QueryDefinition } from './queryDefinition';

export interface NlParseRequest {
  text: string;
  tenantId: string;
}

export interface NlParseResponse {
  /** The canonical definition the user reviews, then runs through `POST /api/search`. */
  definition: QueryDefinition;
  /** Hebrew read-back from the server's renderer — the client never composes one. */
  interpretationText: string;
  /** 0..1 — an indication only; `unresolved` is the signal that matters. */
  confidence: number;
  /** Words the parser could not map. Empty when everything was understood. */
  unresolved: string[];
}
