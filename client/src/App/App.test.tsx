import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it, vi } from 'vitest';
import { queryClient } from '../state/queryClient';
import { App } from './App';

// The feature screens fetch on mount; the shell tests don't exercise those paths.
vi.mock('../api/metadataApi', () => ({ metadataApi: { get: () => new Promise(() => {}) } }));
vi.mock('../api/savedQueriesApi', () => ({ savedQueriesApi: { list: () => new Promise(() => {}) } }));

function renderAt(path: string) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('App shell', () => {
  it('shows one nav item per route and no dropped routes', () => {
    renderAt('/search');
    for (const label of ['חיפוש', 'שאילתות שמורות', 'שאלה חופשית']) {
      expect(screen.getByRole('menuitem', { name: label })).toBeTruthy();
    }
    expect(screen.queryByRole('menuitem', { name: 'תוצאות' })).toBeNull();
  });

  it('mounts each feature route with its heading', () => {
    for (const [path, heading] of [
      ['/saved-queries', 'שאילתות שמורות'], // real screen since S5
      ['/nl-query', 'שאלה חופשית'], // real screen since S6
    ] as const) {
      renderAt(path);
      expect(screen.getByRole('heading', { name: heading })).toBeTruthy();
    }
  });

  it('redirects an unknown path to the first route (search)', () => {
    renderAt('/does-not-exist');
    // The saved-queries / nl-query placeholders are gone; the search screen is mounted instead.
    expect(screen.queryByRole('heading', { name: 'שאילתות שמורות' })).toBeNull();
    expect(screen.getByRole('menuitem', { name: 'חיפוש' })).toBeTruthy();
  });
});
