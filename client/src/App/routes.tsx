import type { ReactNode } from 'react';
import { SearchPage } from '../features/search/SearchPage';
import { SavedQueriesPage } from '../features/saved-queries/SavedQueriesPage';
import { NlQueryPage } from '../features/nl-query/NlQueryPage';
import { t } from '../i18n';

export interface AppRoute {
  path: string;
  label: string;
  element: ReactNode;
}

/** The feature screens. `/` redirects to the first. Results are shown inline on the search screen. */
export const routes: AppRoute[] = [
  { path: '/search', label: t.routes.search, element: <SearchPage /> },
  { path: '/saved-queries', label: t.routes.savedQueries, element: <SavedQueriesPage /> },
  { path: '/nl-query', label: t.routes.nlQuery, element: <NlQueryPage /> },
];
