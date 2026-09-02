import type { ReactNode } from 'react';
import { SearchPage } from '../features/search/SearchPage';
import { ResultsPage } from '../features/results/ResultsPage';
import { SavedQueriesPage } from '../features/saved-queries/SavedQueriesPage';
import { NlQueryPage } from '../features/nl-query/NlQueryPage';

export interface AppRoute {
  path: string;
  label: string;
  element: ReactNode;
}

/** The four feature screens. `/` redirects to the first. */
export const routes: AppRoute[] = [
  { path: '/search', label: 'חיפוש', element: <SearchPage /> },
  { path: '/results', label: 'תוצאות', element: <ResultsPage /> },
  { path: '/saved-queries', label: 'שאילתות שמורות', element: <SavedQueriesPage /> },
  { path: '/nl-query', label: 'שאלה חופשית', element: <NlQueryPage /> },
];
