import { useState } from 'react';
import { Alert, Card, Space, Typography } from 'antd';
import { StarOutlined } from '@ant-design/icons';
import { DEFAULT_TENANT_ID } from '../../../api/config';
import { PageLoader } from '../../../components/PageLoader';
import type { MetadataResponse } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SavedQuery } from '../../../models/savedQuery';
import type { SearchResponse } from '../../../models/search';
import { formatCurrencyIls, formatIntHe } from '../../../lib/format';
import { ResultsPanel } from '../../results/ResultsPanel';
import { useMetadata } from '../../../hooks/useMetadata';
import { RenameQueryModal } from '../RenameQueryModal';
import { SavedQueriesTable } from '../SavedQueriesTable';
import { summarizeRun } from '../runSummary';
import { useSavedQueries } from '../hooks/useSavedQueries';

/** S5 screen: list saved queries, re-run / rename / delete them. Saving happens on the search screen. */
export function SavedQueriesPage() {
  const { list, rename, remove, run } = useSavedQueries();
  const { data: metadata } = useMetadata(DEFAULT_TENANT_ID);
  const [renaming, setRenaming] = useState<SavedQuery | null>(null);
  const rows = list.data ?? [];
  const ranQuery = rows.find((q) => q.id === run.variables);

  return (
    <Space direction="vertical" size={20} style={{ display: 'flex' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        <Space size={10}>
          <StarOutlined aria-hidden />
          שאילתות שמורות
        </Space>
      </Typography.Title>

      {run.data && (
        <RunResult response={run.data} definition={ranQuery?.definition} metadata={metadata} />
      )}

      {list.isLoading ? (
        <Card>
          <PageLoader />
        </Card>
      ) : list.error ? (
        <Alert type="error" showIcon message="טעינת השאילתות השמורות נכשלה" />
      ) : rows.length === 0 ? (
        <Alert type="info" showIcon message="עדיין אין שאילתות שמורות. שמור אחת ממסך החיפוש." />
      ) : (
        <Card styles={{ body: { padding: 0 } }}>
          <SavedQueriesTable
            rows={rows}
            runningId={run.isPending ? (run.variables ?? null) : null}
            onRun={(id) => run.mutate(id)}
            onRename={setRenaming}
            onDelete={(id) => remove.mutate(id)}
          />
        </Card>
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
    </Space>
  );
}

/**
 * A re-run's result: the server's readable question + record/approved/group headline, then the full
 * results (chart + table) once metadata is available. Paging and sorting aren't offered here — the
 * run endpoint takes no definition override (`api-contract.md` §5); the search screen is where a
 * query is adjusted.
 */
function RunResult({
  response,
  definition,
  metadata,
}: {
  response: SearchResponse;
  definition: QueryDefinition | undefined;
  metadata: MetadataResponse | undefined;
}) {
  const { records, approved, groups } = summarizeRun(response);
  const parts = [
    `${formatIntHe(records)} רשומות`,
    `סכום מאושר ${formatCurrencyIls(approved)}`,
    ...(groups > 1 ? [`${groups} קבוצות`] : []),
  ];

  return (
    <div style={{ marginBottom: 16 }}>
      <Alert
        style={{ marginBottom: 16 }}
        type="success"
        showIcon
        message={response.questionText}
        description={parts.join(' · ')}
      />
      {definition && metadata && (
        <ResultsPanel
          response={response}
          error={undefined}
          isFetching={false}
          registry={metadata.filterFieldRegistry}
          references={metadata.references}
          definition={definition}
        />
      )}
    </div>
  );
}
