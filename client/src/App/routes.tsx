import type { ReactNode } from 'react';
import { SearchPage } from '../features/search/SearchPage';
import { SavedQueriesPage } from '../features/saved-queries/SavedQueriesPage';
import { NlQueryPage } from '../features/nl-query/NlQueryPage';

export interface AppRoute {
  path: string;
  label: string;
  element: ReactNode;
}

/** The feature screens. `/` redirects to the first. Results are shown inline on the search screen. */
export const routes: AppRoute[] = [
  { path: '/search', label: 'חיפוש', element: <SearchPage /> },
  { path: '/saved-queries', label: 'שאילתות שמורות', element: <SavedQueriesPage /> },
  { path: '/nl-query', label: 'שאלה חופשית', element: <NlQueryPage /> },
];
