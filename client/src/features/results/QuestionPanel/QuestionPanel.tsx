import { Card, Skeleton, Tag, Typography } from 'antd';

interface Props {
  /** The server's Hebrew read-back of the current query (`SearchResponse.questionText`). */
  text: string | undefined;
  isFetching: boolean;
}

/** Live "readable question" panel — echoes what the server understood the query to mean. */
export function QuestionPanel({ text, isFetching }: Props) {
  return (
    <Card
      size="small"
      title="השאלה"
      extra={isFetching ? <Tag color="processing">מעדכן…</Tag> : null}
      style={{ marginBottom: 16 }}
    >
      {text ? (
        <Typography.Text>{text}</Typography.Text>
      ) : isFetching ? (
        <Skeleton active paragraph={false} />
      ) : (
        <Typography.Text type="secondary">בחר/י פילטרים כדי לראות את השאלה.</Typography.Text>
      )}
    </Card>
  );
}
