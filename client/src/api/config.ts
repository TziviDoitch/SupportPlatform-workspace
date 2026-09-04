/**
 * PoC identity seam. The server has no `/api/auth/login` yet — it derives the authoritative tenant
 * and role from the `X-User` header (`docs/contracts/api-contract.md` §Auth, `ARCHITECTURE.md`
 * §8.1). Until a real login lands, the client just picks one of the seeded users to act as; the
 * header carries the choice and the tenant claim follows from it.
 */

export type UserRole = 'analyst' | 'admin';

export interface SeedUser {
  username: string;
  /** The user's tenant — sent as the `tenantId` claim; the server 403s on a mismatch. */
  tenantId: string;
  role: UserRole;
  /** Hebrew label for the header picker. */
  label: string;
}

/** The users created by the server `DbSeeder`. Keep in sync with `server/.../DbSeeder.cs`. */
export const SEED_USERS: SeedUser[] = [
  { username: 'sarah', tenantId: 'culture-sport-admin', role: 'analyst', label: 'שרה · התרבות והספורט · אנליסטית' },
  { username: 'dan', tenantId: 'culture-sport-admin', role: 'admin', label: 'דן · התרבות והספורט · מנהל' },
  { username: 'michal', tenantId: 'welfare-admin', role: 'analyst', label: 'מיכל · הרווחה · אנליסטית' },
];

export const DEFAULT_USERNAME = 'sarah';
