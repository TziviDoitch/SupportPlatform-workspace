import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { SearchResponse } from '../../../models/search';
import { ResultsChart } from './ResultsChart';

// jsdom has no canvas — stand in for the chart so we test the wiring, not Chart.js.
vi.mock('../../../components/BarChart', () => ({
  BarChart: ({ labels }: { labels: string[] }) => <div data-testid="bar-chart">{labels.join(',')}</div>,
}));

const registry: FilterFieldRegistryEntry[] = [
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
];

const references: References = {
  domains: [],
  bodyTypes: [],
  statuses: [],
  districts: [{ code: 'north', label: 'צפון' }],
};

const response: SearchResponse = {
  questionText: 'q',
  rows: [],
  aggregations: [{ key: { district: 'north' }, metrics: { count: 13 } }],
  page: { pageNumber: 1, pageSize: 50, totalRows: 1 },
  executionMeta: { durationMs: 1, rowCount: 1, cacheHit: false, definitionHash: 'x' },
};

describe('ResultsChart', () => {
  it('renders a chart when the query is segmented by one field', () => {
    render(
      <ResultsChart response={response} segmentation={['district']} registry={registry} references={references} />,
    );
    expect(screen.getByTestId('bar-chart').textContent).toBe('צפון');
  });

  it('renders nothing without a response', () => {
    const { container } = render(
      <ResultsChart response={undefined} segmentation={['district']} registry={registry} references={references} />,
    );
    expect(container.firstChild).toBeNull();
  });

  it('renders nothing when the query is not segmented by exactly one field', () => {
    const { container } = render(
      <ResultsChart response={response} segmentation={[]} registry={registry} references={references} />,
    );
    expect(container.firstChild).toBeNull();
  });
});
