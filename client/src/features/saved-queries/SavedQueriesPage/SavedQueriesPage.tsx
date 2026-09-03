import { useState } from 'react';
import { Alert, Card, Spin, Typography } from 'antd';
import type { SavedQuery } from '../../../models/savedQuery';
import { RenameQueryModal } from '../RenameQueryModal';
import { SavedQueriesTable } from '../SavedQueriesTable';
import { useSavedQueries } from '../hooks/useSavedQueries';

/** S5 screen: list saved queries, re-run / rename / delete them. Saving happens on the search screen. */
export function SavedQueriesPage() {
  const { list, rename, remove, run } = useSavedQueries();
  const [renaming, setRenaming] = useState<SavedQuery | null>(null);
  const rows = list.data ?? [];

  return (
    <Card size="small">
      <Typography.Title level={4} style={{ marginTop: 0 }}>
        שאילתות שמורות
      </Typography.Title>

      {run.data && (
        <Alert
          style={{ marginBottom: 16 }}
          type="success"
          showIcon
          message={run.data.questionText}
          description={`סה"כ שורות: ${run.data.page.totalRows}`}
        />
      )}

      {list.isLoading ? (
        <Spin />
      ) : list.error ? (
        <Alert type="error" showIcon message="טעינת השאילתות השמורות נכשלה" />
      ) : rows.length === 0 ? (
        <Alert type="info" showIcon message="עדיין אין שאילתות שמורות. שמור אחת ממסך החיפוש." />
      ) : (
        <SavedQueriesTable
          rows={rows}
          runningId={run.isPending ? (run.variables ?? null) : null}
          onRun={(id) => run.mutate(id)}
          onRename={setRenaming}
          onDelete={(id) => remove.mutate(id)}
        />
      )}

      <RenameQueryModal
        key={renaming?.id ?? 'none'}
        query={renaming}
        confirmLoading={rename.isPending}
        onCancel={() => setRenaming(null)}
        onSubmit={(name) => {
          if (!renaming) return;
          rename.mutate(
            { id: renaming.id, body: { name, definition: renaming.definition } },
            { onSuccess: () => setRenaming(null) },
          );
        }}
      />
    </Card>
  );
}
