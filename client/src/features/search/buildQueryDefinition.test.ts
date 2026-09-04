import { describe, expect, it } from 'vitest';
import type { FilterFieldRegistryEntry } from '../../models/metadata';
import {
  buildQueryDefinition,
  emptyFormState,
  MIN_YEAR,
  type SearchFormState,
} from './buildQueryDefinition';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'bodyType', label: 'סוג גוף', kind: 'codeList', referenceList: 'bodyTypes', operators: ['in'], segmentable: true },
  { id: 'supportDomain', label: 'תחום תמיכה', kind: 'codeList', referenceList: 'domains', operators: ['in'], segmentable: true },
  { id: 'status', label: 'סטטוס', kind: 'codeList', referenceList: 'statuses', operators: ['in'], segmentable: false },
  { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const DEFAULT_SORT = [
  { field: 'supportYear', direction: 'desc' },
  { field: 'sumAmountApproved', direction: 'desc' },
];

const state = (patch: Partial<SearchFormState>): SearchFormState => ({ ...emptyFormState, ...patch });

describe('buildQueryDefinition', () => {
  it('uses the fixed 3-way breakdown, both metrics, paging + default sort', () => {
    const def = buildQueryDefinition(emptyFormState, registry, 'culture-sport-admin');
    expect(def).toEqual({
      tenantId: 'culture-sport-admin',
      filters: {},
      segmentation: ['supportDomain', 'district', 'supportYear'],
      metrics: ['count', 'sumAmountApproved'],
      paging: { pageNumber: 1, pageSize: 50 },
      sort: DEFAULT_SORT,
    });
  });

  it('graph-field picks do not change the table breakdown', () => {
    const def = buildQueryDefinition(state({ graphFields: ['bodyType'] }), registry, 't');
    expect(def.segmentation).toEqual(['supportDomain', 'district', 'supportYear']);
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

  it('treats "to" without "from" as up-to-and-including, not as that single year', () => {
    expect(
      buildQueryDefinition(state({ values: { supportYear: { to: 2025 } } }), registry, 't')
        .filters.supportYear,
    ).toEqual({ type: 'range', from: MIN_YEAR, to: 2025 });
  });

  it('passes an explicit page and sort through, overriding the default sort', () => {
    const def = buildQueryDefinition(
      state({ pageNumber: 3, pageSize: 25, sort: [{ field: 'count', direction: 'desc' }] }),
      registry,
      't',
    );
    expect(def.paging).toEqual({ pageNumber: 3, pageSize: 25 });
    expect(def.sort).toEqual([{ field: 'count', direction: 'desc' }]);
  });
});
