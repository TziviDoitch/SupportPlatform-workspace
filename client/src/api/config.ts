/**
 * Tenant the client operates as. Temporary S3 placeholder: there is no `/api/auth/login` yet
 * and the server does not enforce identity. S8 replaces this with the authenticated user's tenant.
 */
export const DEFAULT_TENANT_ID = 'culture-sport-admin';

/**
 * User the client acts as, sent as the `X-User` header (`docs/contracts/api-contract.md` §Auth).
 * PoC seam for saved-query ownership + audit until JWT auth lands in S8.
 */
export const DEFAULT_USER = 'sarah';
