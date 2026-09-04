import { Button, Popconfirm, Space } from 'antd';
import type { TableProps } from 'antd';
import { DataTable } from '../../../components/DataTable';
import { formatDateHe } from '../../../lib/format';
import type { SavedQuery } from '../../../models/savedQuery';
import { t } from '../../../i18n';

interface Props {
  rows: SavedQuery[];
  loading?: boolean;
  runningId?: string | null;
  onRun: (id: string) => void;
  onRename: (query: SavedQuery) => void;
  onDelete: (id: string) => void;
}

export const SavedQueriesTable = ({ rows, loading, runningId, onRun, onRename, onDelete }: Props) => {
  const columns: TableProps<SavedQuery>['columns'] = [
    { title: t.savedQueries.tableName, dataIndex: 'name', key: 'name' },
    {
      title: t.savedQueries.tableLastRun,
      key: 'lastRunAt',
      render: (_, row) => formatDateHe(row.lastRunAt),
    },
    {
      title: t.savedQueries.tableRowCount,
      dataIndex: 'lastRunRowCount',
      key: 'lastRunRowCount',
      render: (value: number | null) => value ?? '—',
    },
    {
      title: t.savedQueries.tableActions,
      key: 'actions',
      render: (_, row) => (
        <Space>
          <Button
            size="small"
            type="primary"
            loading={runningId === row.id}
            onClick={() => onRun(row.id)}
          >
            {t.savedQueries.runButton}
          </Button>
          <Button size="small" onClick={() => onRename(row)}>
            {t.savedQueries.renameButton}
          </Button>
          <Popconfirm
            title={t.savedQueries.deleteConfirm}
            okText={t.common.delete}
            cancelText={t.common.cancel}
            onConfirm={() => onDelete(row.id)}
          >
            <Button size="small" danger>
              {t.common.delete}
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return <DataTable<SavedQuery> rows={rows} columns={columns} rowKey="id" loading={loading} pagination={false} />;
};
