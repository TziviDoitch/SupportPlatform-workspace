import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { MetadataResponse } from '../../../models/metadata';
import type { NlParseResponse } from '../../../models/nlQuery';
import { NlQueryPage } from './NlQueryPage';

const metadata: MetadataResponse = {
  tenantId: 'culture-sport-admin',
  references: {
    domains: [{ code: 'culture', label: 'תרבות' }],
    bodyTypes: [],
    statuses: [],
    districts: [{ code: 'north', label: 'צפון' }],
  },
  filterFieldRegistry: [
    { id: 'supportDomain', label: 'תחום תמיכה', kind: 'codeList', referenceList: 'domains', operators: ['in'], segmentable: true },
    { id: 'district', label: 'מחוז', kind: 'codeList', referenceList: 'districts', operators: ['in'], segmentable: true },
  ],
};

const parsed: NlParseResponse = {
  definition: {
    tenantId: 'culture-sport-admin',
    filters: { supportDomain: ['culture'] },
    segmentation: ['district'],
    metrics: ['count', 'sumAmountApproved'],
    paging: { pageNumber: 1, pageSize: 50 },
    sort: [],
  },
  interpretationText: 'כמה בקשות תמיכה עם תחום תמיכה: תרבות, בפילוח לפי מחוז?',
  confidence: 1,
  unresolved: [],
};

const parse = vi.fn();
const runSearch = vi.fn();

vi.mock('../../../api/metadataApi', () => ({ metadataApi: { get: () => Promise.resolve(metadata) } }));
vi.mock('../../../api/nlQueryApi', () => ({ nlQueryApi: { parse: (body: unknown) => parse(body) } }));
vi.mock('../../../api/searchApi', () => ({ searchApi: { run: (d: unknown) => runSearch(d) } }));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={client}>
      <ConfigProvider locale={heIL}>
        <NlQueryPage />
      </ConfigProvider>
    </QueryClientProvider>,
  );
}

async function ask(question: string) {
  renderPage();
  const box = await screen.findByLabelText('שאלה חופשית');
  fireEvent.change(box, { target: { value: question } });
  fireEvent.click(screen.getByRole('button', { name: 'פרש שאלה' }));
}

describe('NlQueryPage', () => {
  beforeEach(() => {
    parse.mockReset().mockResolvedValue(parsed);
    runSearch.mockReset().mockResolvedValue({
      questionText: parsed.interpretationText,
      rows: [{ district: 'north', count: 4 }],
      aggregations: [{ key: { district: 'north' }, metrics: { count: 4 } }],
      page: { pageNumber: 1, pageSize: 50, totalRows: 1 },
      executionMeta: { durationMs: 3, rowCount: 1, cacheHit: false, definitionHash: 'sha256:x' },
    });
  });

  it('shows the interpretation and does not run the search until the user says so', async () => {
    await ask('כמה בקשות בתחום התרבות לפי מחוז');

    expect(await screen.findByText(parsed.interpretationText)).toBeTruthy();
    expect(screen.getByText('תרבות')).toBeTruthy();
    expect(screen.getByText('גרף לפי')).toBeTruthy();
    expect(runSearch).not.toHaveBeenCalled();
  });

  it('runs the parsed definition only when Run is clicked', async () => {
    await ask('כמה בקשות בתחום התרבות לפי מחוז');
    fireEvent.click(await screen.findByRole('button', { name: 'הרץ' }));

    await waitFor(() => expect(runSearch).toHaveBeenCalledWith(parsed.definition));
  });

  it('reports words the parser could not map', async () => {
    parse.mockResolvedValue({ ...parsed, unresolved: ['אשכולות'], confidence: 0.4 });

    await ask('כמה אשכולות');

    expect(await screen.findByText(/אשכולות/)).toBeTruthy();
  });

  it('warns when nothing was understood instead of silently running everything', async () => {
    parse.mockResolvedValue({
      ...parsed,
      definition: { ...parsed.definition, filters: {}, segmentation: [] },
      unresolved: ['בלגן'],
      confidence: 0,
    });

    await ask('בלגן');

    expect(await screen.findByText('לא זוהו סינונים בשאלה')).toBeTruthy();
  });

  it('does not parse an empty question', async () => {
    renderPage();
    await screen.findByLabelText('שאלה חופשית');

    expect(screen.getByRole('button', { name: 'פרש שאלה' }).hasAttribute('disabled')).toBe(true);
    expect(parse).not.toHaveBeenCalled();
  });
});
