import type { ReactElement } from 'react';
import { render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { ResultsPanel } from './ResultsPanel';

vi.mock('../../../components/BarChart', () => ({
  BarChart: ({ labels }: { labels: string[] }) => <div data-testid="bar-chart">{labels.join(',')}</div>,
}));

const registry: FilterFieldRegistryEntry[] = [
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
];
const references: References = { domains: [], bodyTypes: [], statuses: [], districts: [{ code: 'north', label: 'צפון' }] };

const base: SearchResponse = {
  questionText: 'q',
  rows: [{ district: 'north', count: 13 }],
  aggregations: [{ key: { district: 'north' }, metrics: { count: 13 } }],
  page: { pageNumber: 1, pageSize: 50, totalRows: 1 },
  executionMeta: { durationMs: 1, rowCount: 1, cacheHit: false, definitionHash: 'x' },
};

const def = (segmentation: string[]): QueryDefinition => ({
  tenantId: 't',
  filters: {},
  segmentation,
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
});

const wrap = (ui: ReactElement) => render(<ConfigProvider locale={heIL}>{ui}</ConfigProvider>);

describe('ResultsPanel', () => {
  it('shows the chart beside the table when segmented by one field', () => {
    wrap(
      <ResultsPanel
        response={base}
        error={undefined}
        isFetching={false}
        registry={registry}
        references={references}
        definition={def(['district'])}
      />,
    );
    expect(screen.getByTestId('bar-chart')).toBeTruthy();
    expect(screen.getByText('תוצאות')).toBeTruthy();
  });

  it('shows only the table when the query is not segmented', () => {
    wrap(
      <ResultsPanel
        response={{ ...base, segmentation: undefined } as SearchResponse}
        error={undefined}
        isFetching={false}
        registry={registry}
        references={references}
        definition={def([])}
      />,
    );
    expect(screen.queryByTestId('bar-chart')).toBeNull();
  });

  it('renders an error banner instead of results', () => {
    wrap(
      <ResultsPanel
        response={undefined}
        error={new Error('boom')}
        isFetching={false}
        registry={registry}
        references={references}
        definition={def(['district'])}
      />,
    );
    expect(screen.getByText('החיפוש נכשל')).toBeTruthy();
    expect(screen.queryByText('תוצאות')).toBeNull();
  });
});
