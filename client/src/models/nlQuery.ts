import type { QueryDefinition } from './queryDefinition';

export interface NlParseRequest {
  text: string;
  tenantId: string;
}

export interface NlParseResponse {
  definition: QueryDefinition;
  interpretationText: string;
  confidence: number;
  unresolved: string[];
}
