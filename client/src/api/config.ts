// PoC identity seam. The server has no /api/auth/login yet — it derives the authoritative tenant
// and role from the X-User header. Until a real login lands, the client picks one of the seeded
// users to act as.

export type UserRole = 'analyst' | 'admin';

export interface SeedUser {
  username: string;
  tenantId: string;
  role: UserRole;
  label: string;
}

// Keep in sync with the server DbSeeder.
export const SEED_USERS: SeedUser[] = [
  { username: 'sarah', tenantId: 'culture-sport-admin', role: 'analyst', label: 'שרה · התרבות והספורט · אנליסטית' },
  { username: 'dan', tenantId: 'culture-sport-admin', role: 'admin', label: 'דן · התרבות והספורט · מנהל' },
  { username: 'michal', tenantId: 'welfare-admin', role: 'analyst', label: 'מיכל · הרווחה · אנליסטית' },
];

export const DEFAULT_USERNAME = 'sarah';
