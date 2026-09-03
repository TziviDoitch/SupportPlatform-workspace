import { Alert, Button, Card, Descriptions, Space, Typography } from 'antd';
import type { MetadataResponse } from '../../../models/metadata';
import type { NlParseResponse } from '../../../models/nlQuery';
import { describeDefinition } from '../describeDefinition';

interface Props {
  parsed: NlParseResponse;
  metadata: MetadataResponse;
  onRun: () => void;
  isRunning: boolean;
}

/**
 * What the parser understood, shown before anything is executed: the server's read-back sentence,
 * the resulting filters field by field, and anything it could not map. Running is the user's
 * explicit click — nothing here triggers a search.
 */
export function InterpretationPanel({ parsed, metadata, onRun, isRunning }: Props) {
  const fields = describeDefinition(parsed.definition, metadata);

  return (
    <Card size="small" title="פירשתי את הבקשה כך" style={{ marginBottom: 16 }}>
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
            message="לא זוהו סינונים בשאלה"
            description="הרצה כזו תחזיר את כל הבקשות. אפשר לנסח מחדש או להשתמש במסך החיפוש."
          />
        )}

        {parsed.unresolved.length > 0 && (
          <Alert
            type="info"
            showIcon
            message="מילים שלא זוהו"
            description={`${parsed.unresolved.join(', ')} — לא נוספו לשאילתה.`}
          />
        )}

        <Button type="primary" onClick={onRun} loading={isRunning}>
          הרץ
        </Button>
      </Space>
    </Card>
  );
}
