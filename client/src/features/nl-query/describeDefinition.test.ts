import { describe, expect, it } from 'vitest';
import type { MetadataResponse } from '../../models/metadata';
import type { QueryDefinition } from '../../models/queryDefinition';
import { describeDefinition } from './describeDefinition';

const metadata: MetadataResponse = {
  tenantId: 'culture-sport-admin',
  references: {
    domains: [
      { code: 'culture', label: 'תרבות' },
      { code: 'sport', label: 'ספורט' },
    ],
    bodyTypes: [],
    statuses: [],
    districts: [{ code: 'north', label: 'צפון' }],
  },
  filterFieldRegistry: [
    { id: 'supportDomain', label: 'תחום תמיכה', kind: 'codeList', referenceList: 'domains', operators: ['in'], segmentable: true },
    { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
    { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
  ],
};

const base: QueryDefinition = {
  tenantId: 'culture-sport-admin',
  filters: {},
  segmentation: [],
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};

describe('describeDefinition', () => {
  it('shows reference labels, not codes, in registry order', () => {
    const fields = describeDefinition(
      { ...base, filters: { district: ['north'], supportDomain: ['culture', 'sport'] } },
      metadata,
    );

    expect(fields).toEqual([
      { label: 'תחום תמיכה', value: 'תרבות או ספורט' },
      { label: 'מחוז', value: 'צפון' },
    ]);
  });

  it('renders a single year and a year range', () => {
    expect(
      describeDefinition({ ...base, filters: { supportYear: { type: 'single', value: 2024 } } }, metadata),
    ).toEqual([{ label: 'שנת תמיכה', value: '2024' }]);

    expect(
      describeDefinition({ ...base, filters: { supportYear: { type: 'range', from: 2023, to: 2025 } } }, metadata),
    ).toEqual([{ label: 'שנת תמיכה', value: '2023–2025' }]);
  });

  it('adds the grouping fields last', () => {
    const fields = describeDefinition({ ...base, segmentation: ['district'] }, metadata);

    expect(fields).toEqual([{ label: 'גרף לפי', value: 'מחוז' }]);
  });

  it('describes an empty definition as no fields at all', () => {
    expect(describeDefinition(base, metadata)).toEqual([]);
  });
});
