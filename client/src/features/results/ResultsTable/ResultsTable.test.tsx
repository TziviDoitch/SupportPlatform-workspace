import type { ReactElement } from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { ResultsTable } from './ResultsTable';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const references: References = { domains: [], bodyTypes: [], statuses: [], districts: [] };

const definition: QueryDefinition = {
  tenantId: 't',
  filters: {},
  segmentation: ['supportYear'],
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};

const response: SearchResponse = {
  questionText: 'כמה בקשות תמיכה, בפילוח לפי שנת תמיכה?',
  rows: [
    { supportYear: 2023, count: 12 },
    { supportYear: 2024, count: 7 },
  ],
  aggregations: [],
  page: { pageNumber: 1, pageSize: 50, totalGroups: 2 },
  executionMeta: { durationMs: 4, rowCount: 2, cacheHit: false, definitionHash: 'sha256:x' },
};

const noop = vi.fn();

const renderTable = (ui: ReactElement) =>
  render(<ConfigProvider locale={heIL}>{ui}</ConfigProvider>);

describe('ResultsTable', () => {
  it('derives columns from segmentation + metrics and renders the row values', () => {
    renderTable(
      <ResultsTable
        response={response}
        registry={registry}
        references={references}
        definition={definition}
        onPageChange={noop}
        onSortChange={noop}
      />,
    );
    expect(screen.getByText('שנת תמיכה')).toBeTruthy(); // segmentation label from the registry
    expect(screen.getByText('כמות')).toBeTruthy(); // metric label
    expect(screen.getByText('2023')).toBeTruthy();
    expect(screen.getByText('12')).toBeTruthy();
  });

  it('sums the approved amount over every aggregation, not just the rows on this page', () => {
    // `rows` is the page (2 of 3 groups); `aggregations` is the whole result. The header total
    // must come from the latter — otherwise paging silently changes the "total".
    renderTable(
      <ResultsTable
        response={{
          ...response,
          rows: [
            { supportYear: 2023, count: 12, sumAmountApproved: 1000 },
            { supportYear: 2024, count: 7, sumAmountApproved: 2000 },
          ],
          aggregations: [
            { key: { supportYear: 2023 }, metrics: { count: 12, sumAmountApproved: 1000 } },
            { key: { supportYear: 2024 }, metrics: { count: 7, sumAmountApproved: 2000 } },
            { key: { supportYear: 2025 }, metrics: { count: 3, sumAmountApproved: 4000 } },
          ],
          page: { pageNumber: 1, pageSize: 2, totalGroups: 3 },
        }}
        registry={registry}
        references={references}
        definition={{ ...definition, metrics: ['count', 'sumAmountApproved'] }}
        onPageChange={noop}
        onSortChange={noop}
      />,
    );

    // 1000 + 2000 + 4000 — including the group that is not on this page.
    expect(screen.getByText(/7,000/)).toBeTruthy();
    // ...and not 1000 + 2000, which is what summing only the page would give.
    expect(screen.queryByText(/3,000/)).toBeNull();
  });

  it('maps a sort-header click to onSortChange (server-side sort)', () => {
    const onSortChange = vi.fn();
    renderTable(
      <ResultsTable
        response={response}
        registry={registry}
        references={references}
        definition={definition}
        onPageChange={noop}
        onSortChange={onSortChange}
      />,
    );
    fireEvent.click(screen.getByText('כמות'));
    expect(onSortChange).toHaveBeenCalledWith([{ field: 'count', direction: 'asc' }]);
  });

  it('renders read-only (no pager, non-sortable headers) when no handlers are passed', () => {
    renderTable(<ResultsTable response={response} registry={registry} references={references} definition={definition} />);

    // header is plain text, not a sort button
    expect(screen.queryByRole('button', { name: /כמות/ })).toBeNull();
    expect(screen.getByText('כמות')).toBeTruthy();
    // no pagination navigation
    expect(screen.queryByRole('listitem', { name: /1/ })).toBeNull();
  });

  it('shows an empty state when there are no rows', () => {
    renderTable(
      <ResultsTable
        response={{ ...response, rows: [], page: { ...response.page, totalGroups: 0 } }}
        registry={registry}
        references={references}
        definition={definition}
        onPageChange={noop}
        onSortChange={noop}
      />,
    );
    expect(screen.queryByText('12')).toBeNull();
    expect(screen.getAllByText('אין מידע').length).toBeGreaterThan(0); // antd he_IL empty text
  });
});
