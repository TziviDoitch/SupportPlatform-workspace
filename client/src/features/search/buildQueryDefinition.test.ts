import { describe, expect, it } from 'vitest';
import type { FilterFieldRegistryEntry } from '../../models/metadata';
import { buildQueryDefinition, emptyFormState, type SearchFormState } from './buildQueryDefinition';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'bodyType', label: 'סוג גוף', kind: 'codeList', referenceList: 'bodyTypes', operators: ['in'], segmentable: true },
  { id: 'status', label: 'סטטוס', kind: 'codeList', referenceList: 'statuses', operators: ['in'], segmentable: false },
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const state = (patch: Partial<SearchFormState>): SearchFormState => ({ ...emptyFormState, ...patch });

describe('buildQueryDefinition', () => {
  it('omits empty controls and applies the count metric + paging defaults', () => {
    const def = buildQueryDefinition(emptyFormState, registry, 'culture-sport-admin');
    expect(def).toEqual({
      tenantId: 'culture-sport-admin',
      filters: {},
      segmentation: [],
      metrics: ['count'],
      paging: { pageNumber: 1, pageSize: 50 },
      sort: [],
    });
  });

  it('maps a code-list selection to an IN array', () => {
    const def = buildQueryDefinition(state({ values: { bodyType: ['association'] } }), registry, 't');
    expect(def.filters).toEqual({ bodyType: ['association'] });
  });

  it('drops a code-list control whose selection was cleared', () => {
    const def = buildQueryDefinition(state({ values: { bodyType: [] } }), registry, 't');
    expect(def.filters).toEqual({});
  });

  it('builds a year range when both ends are set, a single year when one is', () => {
    expect(
      buildQueryDefinition(state({ values: { supportYear: { from: 2023, to: 2025 } } }), registry, 't')
        .filters.supportYear,
    ).toEqual({ type: 'range', from: 2023, to: 2025 });

    expect(
      buildQueryDefinition(state({ values: { supportYear: { from: 2024 } } }), registry, 't')
        .filters.supportYear,
    ).toEqual({ type: 'single', value: 2024 });
  });

  it('keeps only segmentable ids in segmentation', () => {
    const def = buildQueryDefinition(
      state({ segmentation: ['supportYear', 'status', 'unknown'] }),
      registry,
      't',
    );
    expect(def.segmentation).toEqual(['supportYear']);
  });

  it('passes page and sort through', () => {
    const def = buildQueryDefinition(
      state({ pageNumber: 3, pageSize: 25, sort: [{ field: 'count', direction: 'desc' }] }),
      registry,
      't',
    );
    expect(def.paging).toEqual({ pageNumber: 3, pageSize: 25 });
    expect(def.sort).toEqual([{ field: 'count', direction: 'desc' }]);
  });
});
