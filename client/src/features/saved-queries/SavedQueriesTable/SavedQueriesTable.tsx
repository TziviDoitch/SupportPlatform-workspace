import { useMemo } from 'react';
import { Button, Popconfirm, Space } from 'antd';
import type { TableProps } from 'antd';
import { DataTable } from '../../../components/DataTable';
import type { SavedQuery } from '../../../models/savedQuery';

interface Props {
  rows: SavedQuery[];
  loading?: boolean;
  runningId?: string | null;
  onRun: (id: string) => void;
  onRename: (query: SavedQuery) => void;
  onDelete: (id: string) => void;
}

const formatDate = (iso: string | null): string =>
  iso ? new Date(iso).toLocaleDateString('he-IL') : '—';

/** The saved-queries list with per-row re-run / rename / delete actions. */
export function SavedQueriesTable({ rows, loading, runningId, onRun, onRename, onDelete }: Props) {
  const columns = useMemo<TableProps<SavedQuery>['columns']>(
    () => [
      { title: 'שם', dataIndex: 'name', key: 'name' },
      {
        title: 'הרצה אחרונה',
        key: 'lastRunAt',
        render: (_, row) => formatDate(row.lastRunAt),
      },
      {
        title: 'קבוצות בהרצה האחרונה',
        dataIndex: 'lastRunRowCount',
        key: 'lastRunRowCount',
        render: (value: number | null) => value ?? '—',
      },
      {
        title: 'פעולות',
        key: 'actions',
        render: (_, row) => (
          <Space>
            <Button
              size="small"
              type="primary"
              loading={runningId === row.id}
              onClick={() => onRun(row.id)}
            >
              הרץ
            </Button>
            <Button size="small" onClick={() => onRename(row)}>
              שנה שם
            </Button>
            <Popconfirm
              title="למחוק את השאילתה?"
              okText="מחק"
              cancelText="ביטול"
              onConfirm={() => onDelete(row.id)}
            >
              <Button size="small" danger>
                מחק
              </Button>
            </Popconfirm>
          </Space>
        ),
      },
    ],
    [runningId, onRun, onRename, onDelete],
  );

  return <DataTable<SavedQuery> rows={rows} columns={columns} rowKey="id" loading={loading} pagination={false} />;
}
