import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { ChartData } from '../buildChartData';
import { ResultsChart } from './ResultsChart';

// jsdom has no canvas — stand in for the chart so we test the wiring, not Chart.js.
vi.mock('../../../components/BarChart', () => ({
  BarChart: ({ labels, seriesLabel }: { labels: string[]; seriesLabel: string }) => (
    <div data-testid="bar-chart">{`${seriesLabel}:${labels.join(',')}`}</div>
  ),
}));

const data: ChartData = { labels: ['צפון', 'דרום'], values: [13, 4], seriesLabel: 'מחוז' };

describe('ResultsChart', () => {
  it('frames the pre-built series in a card titled by the field label', () => {
    render(<ResultsChart data={data} />);
    expect(screen.getByText('מחוז')).toBeTruthy(); // card title = seriesLabel
    expect(screen.getByTestId('bar-chart').textContent).toBe('מחוז:צפון,דרום');
  });
});
