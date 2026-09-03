import { describe, expect, it } from 'vitest';
import type { SearchResponse } from '../../models/search';
import { summarizeRun } from './runSummary';

const base: Omit<SearchResponse, 'aggregations' | 'page'> = {
  questionText: 'q',
  rows: [],
  executionMeta: { durationMs: 1, rowCount: 0, cacheHit: false, definitionHash: 'sha256:x' },
};

describe('summarizeRun', () => {
  it('sums count and sumAmountApproved across groups and reports the group count', () => {
    const response: SearchResponse = {
      ...base,
      aggregations: [
        { key: { supportYear: 2023 }, metrics: { count: 12, sumAmountApproved: 1000 } },
        { key: { supportYear: 2024 }, metrics: { count: 7, sumAmountApproved: 500 } },
      ],
      page: { pageNumber: 1, pageSize: 50, totalRows: 2 },
    };

    expect(summarizeRun(response)).toEqual({ records: 19, approved: 1500, groups: 2 });
  });

  it('handles an unsegmented query (one group, the total)', () => {
    const response: SearchResponse = {
      ...base,
      aggregations: [{ key: {}, metrics: { count: 320, sumAmountApproved: 42000 } }],
      page: { pageNumber: 1, pageSize: 50, totalRows: 1 },
    };

    expect(summarizeRun(response)).toEqual({ records: 320, approved: 42000, groups: 1 });
  });
});
