import { useState } from 'react';
import { Alert, Button, Card, Input, Space, Typography } from 'antd';
import { BulbOutlined, EditOutlined, SendOutlined } from '@ant-design/icons';
import { getActiveUser } from '../../../api/activeUser';
import { PageLoader } from '../../../components/PageLoader';
import { SectionTitle } from '../../../components/SectionTitle';
import type { MetadataResponse } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import { withPaging, withSort } from '../../../lib/queryDefinition';
import { ResultsSection } from '../../results/ResultsSection';
import { useSearch } from '../../results/hooks/useSearch';
import { useMetadata } from '../../../hooks/useMetadata';
import { InterpretationPanel } from '../InterpretationPanel';
import { useNlParse } from '../hooks/useNlParse';

const EXAMPLE = 'לדוגמה: כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז';

/** The S6 screen: question → interpretation → the user runs it → the existing search slice. */
export function NlQueryPage() {
  const { data: metadata, isLoading, error } = useMetadata(getActiveUser().tenantId);

  return (
    <Space direction="vertical" size={20} style={{ display: 'flex' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        <SectionTitle icon={<BulbOutlined />}>שאלה חופשית</SectionTitle>
      </Typography.Title>

      {isLoading ? (
        <Card>
          <PageLoader />
        </Card>
      ) : error || !metadata ? (
        <Alert type="error" showIcon message="טעינת נתוני הסינון נכשלה" />
      ) : (
        <NlQueryView metadata={metadata} />
      )}
    </Space>
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
    parse.mutate({ text: question, tenantId: getActiveUser().tenantId });
  };

  const setPage = (pageNumber: number, pageSize: number) =>
    setDefinition((d) => d && withPaging(d, pageNumber, pageSize));

  const setSort = (sort: SortSpec[]) => setDefinition((d) => d && withSort(d, sort));

  return (
    <Space direction="vertical" size={16} style={{ display: 'flex' }}>
      <Card title={<SectionTitle icon={<EditOutlined />}>ניסוח השאלה</SectionTitle>}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Input.TextArea
            rows={2}
            aria-label="שאלה חופשית"
            placeholder={EXAMPLE}
            value={text}
            onChange={(e) => setText(e.target.value)}
          />
          <Button
            type="primary"
            icon={<SendOutlined aria-hidden />}
            onClick={submit}
            loading={parse.isPending}
            disabled={question.length === 0}
          >
            פרש שאלה
          </Button>
        </Space>
      </Card>

      {parse.data && (
        <InterpretationPanel
          parsed={parse.data}
          metadata={metadata}
          onRun={() => setDefinition(parse.data.definition)}
          isRunning={isFetching}
        />
      )}

      {definition && (
        <ResultsSection
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
    </Space>
  );
}
