import { useState } from 'react';
import { Alert, Card, Space, Typography } from 'antd';
import { StarOutlined } from '@ant-design/icons';
import { getActiveUser } from '../../../api/activeUser';
import { PageLoader } from '../../../components/PageLoader';
import { SectionTitle } from '../../../components/SectionTitle';
import type { MetadataResponse } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import type { SavedQuery } from '../../../models/savedQuery';
import type { SearchResponse } from '../../../models/search';
import { ResultsSection } from '../../results/ResultsSection';
import { useMetadata } from '../../../hooks/useMetadata';
import { RenameQueryModal } from '../RenameQueryModal';
import { SavedQueriesTable } from '../SavedQueriesTable';
import { useSavedQueries } from '../hooks/useSavedQueries';
import { t } from '../../../i18n';

export const SavedQueriesPage = () => {
  const { list, rename, remove, run } = useSavedQueries();
  const { data: metadata } = useMetadata(getActiveUser().tenantId);
  const [renaming, setRenaming] = useState<SavedQuery | null>(null);
  const rows = list.data ?? [];
  const ranQuery = rows.find((q) => q.id === run.variables);

  return (
    <Space direction="vertical" size={20} style={{ display: 'flex' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        <SectionTitle icon={<StarOutlined />}>{t.savedQueries.title}</SectionTitle>
      </Typography.Title>

      {run.data && (
        <RunResult response={run.data} definition={ranQuery?.definition} metadata={metadata} />
      )}

      {list.isLoading ? (
        <Card>
          <PageLoader />
        </Card>
      ) : list.error ? (
        <Alert type="error" showIcon message={t.savedQueries.loadError} />
      ) : rows.length === 0 ? (
        <Alert type="info" showIcon message={t.savedQueries.empty} />
      ) : (
        <Card title={<SectionTitle icon={<StarOutlined />}>{t.savedQueries.myQueries}</SectionTitle>} styles={{ body: { padding: 0 } }}>
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
};

const RunResult = ({
  response,
  definition,
  metadata,
}: {
  response: SearchResponse;
  definition: QueryDefinition | undefined;
  metadata: MetadataResponse | undefined;
}) => {
  if (!definition || !metadata) return null;

  return (
    <ResultsSection
      response={response}
      error={undefined}
      isFetching={false}
      registry={metadata.filterFieldRegistry}
      references={metadata.references}
      definition={definition}
    />
  );
};
