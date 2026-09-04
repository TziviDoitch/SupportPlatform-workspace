import { DEFAULT_USERNAME, SEED_USERS, type SeedUser } from './config';

/**
 * The seeded user the client currently acts as. Module-level (not React state) so `http.ts` — which
 * is not a component — can read it for the `X-User` header. The header row picker is the only
 * writer; it also remounts the screens and clears the query cache so nothing leaks between
 * identities. Persisted to `localStorage` so a reload keeps the choice.
 */
const STORAGE_KEY = 'sp.activeUser';

const byName = (username: string | null): SeedUser =>
  SEED_USERS.find((u) => u.username === username) ??
  SEED_USERS.find((u) => u.username === DEFAULT_USERNAME) ??
  SEED_USERS[0];

function load(): SeedUser {
  try {
    return byName(localStorage.getItem(STORAGE_KEY));
  } catch {
    return byName(null);
  }
}

let active: SeedUser = load();

export const getActiveUser = (): SeedUser => active;

export function setActiveUser(username: string): SeedUser {
  active = byName(username);
  try {
    localStorage.setItem(STORAGE_KEY, active.username);
  } catch {
    /* private mode / storage disabled — the choice just won't persist */
  }
  return active;
}
