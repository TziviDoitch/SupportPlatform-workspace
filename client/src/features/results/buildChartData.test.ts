import { describe, expect, it } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../models/metadata';
import type { AggregationRow } from '../../models/search';
import { buildChartData } from './buildChartData';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const references: References = {
  domains: [],
  bodyTypes: [],
  statuses: [],
  districts: [
    { code: 'north', label: 'צפון' },
    { code: 'center', label: 'מרכז' },
  ],
};

const agg = (key: Record<string, string | number>, count: number): AggregationRow => ({
  key,
  metrics: { count, sumAmountApproved: count * 1000 },
});

describe('buildChartData', () => {
  it('resolves code-list bucket labels from the reference list', () => {
    const data = buildChartData(
      [agg({ district: 'north' }, 13), agg({ district: 'center' }, 5)],
      ['district'],
      registry,
      references,
    );
    expect(data).toEqual({ labels: ['צפון', 'מרכז'], values: [13, 5], seriesLabel: 'מחוז' });
  });

  it('shows a yearRange bucket key as-is (no reference list)', () => {
    const data = buildChartData(
      [agg({ supportYear: 2023 }, 8), agg({ supportYear: 2024 }, 9)],
      ['supportYear'],
      registry,
      references,
    );
    expect(data).toEqual({ labels: ['2023', '2024'], values: [8, 9], seriesLabel: 'שנת תמיכה' });
  });

  it('falls back to the raw code when the reference list has no match', () => {
    const data = buildChartData([agg({ district: 'west' }, 2)], ['district'], registry, references);
    expect(data?.labels).toEqual(['west']);
  });

  it('returns null when there is not exactly one segmentation field', () => {
    expect(buildChartData([agg({ district: 'north' }, 1)], [], registry, references)).toBeNull();
    expect(
      buildChartData([agg({ district: 'north', supportYear: 2023 }, 1)], ['district', 'supportYear'], registry, references),
    ).toBeNull();
  });

  it('returns null when there are no aggregation rows', () => {
    expect(buildChartData([], ['district'], registry, references)).toBeNull();
  });

  it('returns null when the segmentation field is not in the registry', () => {
    expect(buildChartData([agg({ ghost: 'x' }, 1)], ['ghost'], registry, references)).toBeNull();
  });
});
