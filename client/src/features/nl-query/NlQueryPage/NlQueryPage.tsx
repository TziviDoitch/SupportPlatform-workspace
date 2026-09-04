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
import { t } from '../../../i18n';

export const NlQueryPage = () => {
  const { data: metadata, isLoading, error } = useMetadata(getActiveUser().tenantId);

  return (
    <Space direction="vertical" size={20} style={{ display: 'flex' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        <SectionTitle icon={<BulbOutlined />}>{t.nlQuery.title}</SectionTitle>
      </Typography.Title>

      {isLoading ? (
        <Card>
          <PageLoader />
        </Card>
      ) : error || !metadata ? (
        <Alert type="error" showIcon message={t.nlQuery.filterError} />
      ) : (
        <NlQueryView metadata={metadata} />
      )}
    </Space>
  );
};

const NlQueryView = ({ metadata }: { metadata: MetadataResponse }) => {
  const [text, setText] = useState('');
  const parse = useNlParse();

  const [definition, setDefinition] = useState<QueryDefinition>();
  const { data, error, isFetching } = useSearch(definition);

  const question = text.trim();

  const submit = () => {
    if (question.length === 0) return;
    setDefinition(undefined);
    parse.mutate({ text: question, tenantId: getActiveUser().tenantId });
  };

  const setPage = (pageNumber: number, pageSize: number) =>
    setDefinition((d) => d && withPaging(d, pageNumber, pageSize));

  const setSort = (sort: SortSpec[]) => setDefinition((d) => d && withSort(d, sort));

  return (
    <Space direction="vertical" size={16} style={{ display: 'flex' }}>
      <Card title={<SectionTitle icon={<EditOutlined />}>{t.nlQuery.questionFormTitle}</SectionTitle>}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Input.TextArea
            rows={2}
            aria-label={t.nlQuery.questionLabel}
            placeholder={t.nlQuery.questionPlaceholder}
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
            {t.nlQuery.parseButton}
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
};
