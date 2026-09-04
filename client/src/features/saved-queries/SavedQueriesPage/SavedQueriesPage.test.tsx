import { render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { SavedQuery } from '../../../models/savedQuery';
import type { SearchResponse } from '../../../models/search';

vi.mock('react-chartjs-2', () => ({ Bar: () => <div data-testid="bar-chart" /> }));

const definition: SavedQuery['definition'] = {
  tenantId: 'culture-sport-admin',
  filters: {},
  segmentation: ['supportYear'],
  metrics: ['count', 'sumAmountApproved'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};

const savedQuery: SavedQuery = {
  id: 'a1',
  name: 'לפי שנה',
  definition,
  ownerUsername: 'sarah',
  tenantId: 'culture-sport-admin',
  createdAt: '2026-01-01T00:00:00Z',
  lastRunAt: null,
  lastRunRowCount: null,
};

const runResponse: SearchResponse = {
  questionText: 'כמה בקשות תמיכה, בפילוח לפי שנת תמיכה?',
  rows: [{ supportYear: 2023, count: 12, sumAmountApproved: 1000 }],
  aggregations: [{ key: { supportYear: 2023 }, metrics: { count: 12, sumAmountApproved: 1000 } }],
  page: { pageNumber: 1, pageSize: 50, totalGroups: 1 },
  executionMeta: { durationMs: 3, rowCount: 1, cacheHit: false, definitionHash: 'x' },
};

vi.mock('../hooks/useSavedQueries', () => ({
  useSavedQueries: () => ({
    list: { data: [savedQuery], isLoading: false, error: null },
    rename: { isPending: false, mutate: vi.fn() },
    remove: { isPending: false, mutate: vi.fn() },
    run: { data: runResponse, variables: 'a1', isPending: false, mutate: vi.fn() },
  }),
}));

vi.mock('../../../hooks/useMetadata', () => ({
  useMetadata: () => ({
    data: {
      tenantId: 'culture-sport-admin',
      references: { domains: [], bodyTypes: [], statuses: [], districts: [] },
      filterFieldRegistry: [
        { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
      ],
    },
  }),
}));

const { SavedQueriesPage } = await import('./SavedQueriesPage');

describe('SavedQueriesPage', () => {
  it('shows the same full results view as the search screen for a re-run', () => {
    render(
      <ConfigProvider locale={heIL}>
        <SavedQueriesPage />
      </ConfigProvider>,
    );

    // the server's readable question (QuestionPanel), same as the search screen
    expect(screen.getByText(runResponse.questionText)).toBeTruthy();
    // full table: the segmentation label + the bucket row from the response
    expect(screen.getAllByText('שנת תמיכה').length).toBeGreaterThan(0);
    expect(screen.getByText('2023')).toBeTruthy();
    expect(screen.getByText('12')).toBeTruthy();
  });
});
