import type { ReactElement } from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { ResultsTable } from './ResultsTable';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

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
  page: { pageNumber: 1, pageSize: 50, totalRows: 2 },
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

  it('maps a sort-header click to onSortChange (server-side sort)', () => {
    const onSortChange = vi.fn();
    renderTable(
      <ResultsTable
        response={response}
        registry={registry}
        definition={definition}
        onPageChange={noop}
        onSortChange={onSortChange}
      />,
    );
    fireEvent.click(screen.getByText('כמות'));
    expect(onSortChange).toHaveBeenCalledWith([{ field: 'count', direction: 'asc' }]);
  });

  it('shows an empty state when there are no rows', () => {
    renderTable(
      <ResultsTable
        response={{ ...response, rows: [], page: { ...response.page, totalRows: 0 } }}
        registry={registry}
        definition={definition}
        onPageChange={noop}
        onSortChange={noop}
      />,
    );
    expect(screen.queryByText('12')).toBeNull();
    expect(screen.getAllByText('אין מידע').length).toBeGreaterThan(0); // antd he_IL empty text
  });
});
