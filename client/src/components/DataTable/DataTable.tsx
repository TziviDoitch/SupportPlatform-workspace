import { Table, type TableProps } from 'antd';

interface DataTableProps<T> {
  columns: TableProps<T>['columns'];
  rows: T[];
  rowKey: TableProps<T>['rowKey'];
  loading?: boolean;
  pagination?: TableProps<T>['pagination'];
  onChange?: TableProps<T>['onChange'];
}

/** Generic antd `Table` wrapper — no domain knowledge. Styling and defaults decided once. */
export function DataTable<T extends object>({
  columns,
  rows,
  rowKey,
  loading,
  pagination,
  onChange,
}: DataTableProps<T>) {
  return (
    <Table<T>
      columns={columns}
      dataSource={rows}
      rowKey={rowKey}
      loading={loading}
      pagination={pagination ?? { pageSize: 50 }}
      onChange={onChange}
      size="middle"
    />
  );
}
