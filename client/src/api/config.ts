/**
 * Tenant the client operates as. Since S8 the server derives the authoritative tenant from
 * identity (the `X-User` header) and returns 403 on a mismatch — this literal just tells the
 * server which tenant we *claim* to be. A real login flow (`/api/auth/login`, JWT) is the
 * documented production target (`docs/ARCHITECTURE.md` §8.1); until then it stays a fixed stand-in.
 */
export const DEFAULT_TENANT_ID = 'culture-sport-admin';

/**
 * User the client acts as, sent as the `X-User` header (`docs/contracts/api-contract.md` §Auth).
 * PoC identity seam for tenant scoping, saved-query ownership and audit until JWT auth lands.
 */
export const DEFAULT_USER = 'sarah';
