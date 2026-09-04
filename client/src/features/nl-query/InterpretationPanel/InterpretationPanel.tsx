import { Alert, Button, Card, Descriptions, Space, Typography } from 'antd';
import { ProfileOutlined } from '@ant-design/icons';
import { SectionTitle } from '../../../components/SectionTitle';
import type { MetadataResponse } from '../../../models/metadata';
import type { NlParseResponse } from '../../../models/nlQuery';
import { describeDefinition } from '../describeDefinition';
import { t } from '../../../i18n';

interface Props {
  parsed: NlParseResponse;
  metadata: MetadataResponse;
  onRun: () => void;
  isRunning: boolean;
}

export const InterpretationPanel = ({ parsed, metadata, onRun, isRunning }: Props) => {
  const fields = describeDefinition(parsed.definition, metadata);

  return (
    <Card size="small" title={<SectionTitle icon={<ProfileOutlined />}>{t.interpretation.title}</SectionTitle>}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Typography.Text>{parsed.interpretationText}</Typography.Text>

        {fields.length > 0 ? (
          <Descriptions size="small" column={1} bordered items={
            fields.map((f) => ({ key: f.label, label: f.label, children: f.value }))
          } />
        ) : (
          <Alert
            type="warning"
            showIcon
            message={t.interpretation.noFilters}
            description={t.interpretation.noFiltersDescription}
          />
        )}

        {parsed.unresolved.length > 0 && (
          <Alert
            type="info"
            showIcon
            message={t.interpretation.unresolvedTitle}
            description={`${parsed.unresolved.join(', ')} ${t.interpretation.unresolvedDescription}`}
          />
        )}

        <Button type="primary" onClick={onRun} loading={isRunning}>
          {t.common.run}
        </Button>
      </Space>
    </Card>
  );
};
