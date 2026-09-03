import { describe, expect, it } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../models/metadata';
import type { AggregationRow } from '../../models/search';
import { buildCharts } from './buildChartData';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
  { id: 'bodyType', label: 'סוג גוף', kind: 'codeList', referenceList: 'bodyTypes', operators: ['in'], segmentable: true },
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const references: References = {
  domains: [],
  bodyTypes: [{ code: 'association', label: 'עמותה' }, { code: 'company', label: 'חברה' }],
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

describe('buildCharts', () => {
  it('one chart for one segmentation field, labels resolved from the reference list', () => {
    const charts = buildCharts(
      [agg({ district: 'north' }, 13), agg({ district: 'center' }, 5)],
      ['district'],
      registry,
      references,
    );
    expect(charts).toEqual([{ labels: ['צפון', 'מרכז'], values: [13, 5], seriesLabel: 'מחוז' }]);
  });

  it('shows a yearRange bucket key as-is (no reference list)', () => {
    const charts = buildCharts(
      [agg({ supportYear: 2023 }, 8), agg({ supportYear: 2024 }, 9)],
      ['supportYear'],
      registry,
      references,
    );
    expect(charts).toEqual([{ labels: ['2023', '2024'], values: [8, 9], seriesLabel: 'שנת תמיכה' }]);
  });

  it('one chart per field, each the marginal sum over the other fields', () => {
    const charts = buildCharts(
      [
        agg({ district: 'north', bodyType: 'association' }, 10),
        agg({ district: 'north', bodyType: 'company' }, 4),
        agg({ district: 'center', bodyType: 'association' }, 6),
      ],
      ['district', 'bodyType'],
      registry,
      references,
    );
    expect(charts).toEqual([
      { labels: ['צפון', 'מרכז'], values: [14, 6], seriesLabel: 'מחוז' },
      { labels: ['עמותה', 'חברה'], values: [16, 4], seriesLabel: 'סוג גוף' },
    ]);
  });

  it('falls back to the raw code when the reference list has no match', () => {
    expect(buildCharts([agg({ district: 'west' }, 2)], ['district'], registry, references)[0].labels).toEqual(['west']);
  });

  it('returns [] when there is no segmentation, no rows, or an unknown field', () => {
    expect(buildCharts([agg({ district: 'north' }, 1)], [], registry, references)).toEqual([]);
    expect(buildCharts([], ['district'], registry, references)).toEqual([]);
    expect(buildCharts([agg({ ghost: 'x' }, 1)], ['ghost'], registry, references)).toEqual([]);
  });
});
