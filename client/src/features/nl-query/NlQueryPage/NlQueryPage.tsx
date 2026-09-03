import { useState } from 'react';
import { Alert, Button, Card, Input, Space, Spin, Typography } from 'antd';
import { DEFAULT_TENANT_ID } from '../../../api/config';
import type { MetadataResponse } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import { ResultsPanel } from '../../results/ResultsPanel';
import { useSearch } from '../../results/hooks/useSearch';
import { useMetadata } from '../../search/hooks/useMetadata';
import { InterpretationPanel } from '../InterpretationPanel';
import { useNlParse } from '../hooks/useNlParse';

const EXAMPLE = 'לדוגמה: כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז';

/** The S6 screen: question → interpretation → the user runs it → the existing search slice. */
export function NlQueryPage() {
  const { data: metadata, isLoading, error } = useMetadata(DEFAULT_TENANT_ID);

  return (
    <Card size="small">
      <Typography.Title level={4} style={{ marginTop: 0 }}>
        שאלה חופשית
      </Typography.Title>

      {isLoading ? (
        <Spin />
      ) : error || !metadata ? (
        <Alert type="error" showIcon message="טעינת נתוני הסינון נכשלה" />
      ) : (
        <NlQueryView metadata={metadata} />
      )}
    </Card>
  );
}

function NlQueryView({ metadata }: { metadata: MetadataResponse }) {
  const [text, setText] = useState('');
  const parse = useNlParse();

  // Set only when the user presses Run, so parsing never executes a search.
  const [definition, setDefinition] = useState<QueryDefinition>();
  const { data, error, isFetching } = useSearch(definition);

  const question = text.trim();

  const submit = () => {
    if (question.length === 0) return;
    setDefinition(undefined); // a new question invalidates the previous results
    parse.mutate({ text: question, tenantId: DEFAULT_TENANT_ID });
  };

  const setPage = (pageNumber: number, pageSize: number) =>
    setDefinition((d) => d && { ...d, paging: { pageNumber, pageSize } });

  const setSort = (sort: SortSpec[]) =>
    setDefinition((d) => d && { ...d, sort, paging: { ...d.paging, pageNumber: 1 } });

  return (
    <>
      <Space direction="vertical" size="middle" style={{ width: '100%', marginBottom: 16 }}>
        <Input.TextArea
          rows={2}
          aria-label="שאלה חופשית"
          placeholder={EXAMPLE}
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <Button
          type="primary"
          onClick={submit}
          loading={parse.isPending}
          disabled={question.length === 0}
        >
          פרש שאלה
        </Button>
      </Space>

      {parse.data && (
        <InterpretationPanel
          parsed={parse.data}
          metadata={metadata}
          onRun={() => setDefinition(parse.data.definition)}
          isRunning={isFetching}
        />
      )}

      {definition && (
        <ResultsPanel
          response={data}
          error={error}
          isFetching={isFetching}
          registry={metadata.filterFieldRegistry}
          references={metadata.references}
          definition={definition}
          onPageChange={setPage}
          onSortChange={setSort}
        />
      )}
    </>
  );
}
