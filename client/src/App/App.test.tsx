import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it } from 'vitest';
import { queryClient } from '../state/queryClient';
import { App } from './App';

function renderAt(path: string) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

// getByRole throws when the element is missing, so it doubles as the assertion.
describe('App routing', () => {
  it('shows the search screen by default', () => {
    renderAt('/');
    expect(screen.getByRole('heading', { name: 'חיפוש' })).toBeTruthy();
  });

  it('renders each feature route', () => {
    for (const [path, heading] of [
      ['/results', 'תוצאות'],
      ['/saved-queries', 'שאילתות שמורות'],
      ['/nl-query', 'שאלה חופשית'],
    ] as const) {
      renderAt(path);
      expect(screen.getByRole('heading', { name: heading })).toBeTruthy();
    }
  });

  it('redirects unknown paths to the search screen', () => {
    renderAt('/does-not-exist');
    expect(screen.getByRole('heading', { name: 'חיפוש' })).toBeTruthy();
  });
});
