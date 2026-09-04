import type { ReactElement } from 'react';
import { render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { ResultsSection } from './ResultsSection';

vi.mock('../../../components/BarChart', () => ({ BarChart: () => <div data-testid="bar-chart" /> }));

const registry: FilterFieldRegistryEntry[] = [
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
];
const references: References = { domains: [], bodyTypes: [], statuses: [], districts: [{ code: 'north', label: 'צפון' }] };
const definition: QueryDefinition = {
  tenantId: 't',
  filters: {},
  segmentation: ['district'],
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};
const response: SearchResponse = {
  questionText: 'כמה בקשות תמיכה, לפי מחוז?',
  rows: [{ district: 'north', count: 7 }],
  aggregations: [{ key: { district: 'north' }, metrics: { count: 7 } }],
  page: { pageNumber: 1, pageSize: 50, totalGroups: 1 },
  executionMeta: { durationMs: 1, rowCount: 1, cacheHit: false, definitionHash: 'x' },
};

const wrap = (ui: ReactElement) => render(<ConfigProvider locale={heIL}>{ui}</ConfigProvider>);
const props = { registry, references, definition };

describe('ResultsSection', () => {
  it('shows a spinner while the first response is loading', () => {
    const { container } = wrap(
      <ResultsSection response={undefined} error={undefined} isFetching {...props} />,
    );
    expect(container.querySelector('.ant-spin')).toBeTruthy();
    expect(screen.queryByText('תוצאות')).toBeNull();
  });

  it('shows the question and the results once a response is in', () => {
    wrap(<ResultsSection response={response} error={undefined} isFetching={false} {...props} />);
    expect(screen.getByText(response.questionText)).toBeTruthy();
    expect(screen.getByText('תוצאות')).toBeTruthy();
    expect(screen.getByTestId('bar-chart')).toBeTruthy();
  });

  it('surfaces an error instead of the results', () => {
    wrap(
      <ResultsSection response={undefined} error={new Error('boom')} isFetching={false} {...props} />,
    );
    expect(screen.getByText('החיפוש נכשל')).toBeTruthy();
  });
});
