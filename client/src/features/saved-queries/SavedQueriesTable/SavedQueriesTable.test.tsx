import type { ReactElement } from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { describe, expect, it, vi } from 'vitest';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SavedQuery } from '../../../models/savedQuery';
import { SavedQueriesTable } from './SavedQueriesTable';

const definition: QueryDefinition = {
  tenantId: 'culture-sport-admin',
  filters: {},
  segmentation: [],
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};

const rows: SavedQuery[] = [
  {
    id: 'a1',
    name: 'עמותות מאושרות',
    definition,
    ownerUsername: 'sarah',
    tenantId: 'culture-sport-admin',
    createdAt: '2026-01-01T00:00:00Z',
    lastRunAt: '2026-01-05T00:00:00Z',
    lastRunRowCount: 3,
  },
];

const renderTable = (ui: ReactElement) => render(<ConfigProvider locale={heIL}>{ui}</ConfigProvider>);

describe('SavedQueriesTable', () => {
  const noop = vi.fn();

  it('renders the name and last-run row count', () => {
    renderTable(
      <SavedQueriesTable rows={rows} onRun={noop} onRename={noop} onDelete={noop} />,
    );
    expect(screen.getByText('עמותות מאושרות')).toBeTruthy();
    expect(screen.getByText('3')).toBeTruthy();
  });

  it('calls onRun with the row id when "הרץ" is clicked', () => {
    const onRun = vi.fn();
    renderTable(
      <SavedQueriesTable rows={rows} onRun={onRun} onRename={noop} onDelete={noop} />,
    );
    fireEvent.click(screen.getByRole('button', { name: 'הרץ' }));
    expect(onRun).toHaveBeenCalledWith('a1');
  });

  it('shows an empty state when there are no rows', () => {
    renderTable(
      <SavedQueriesTable rows={[]} onRun={noop} onRename={noop} onDelete={noop} />,
    );
    expect(screen.getAllByText('אין מידע').length).toBeGreaterThan(0);
  });
});
