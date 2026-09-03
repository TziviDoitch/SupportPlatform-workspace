import { Card, Skeleton, Space, Tag, Typography } from 'antd';
import { MessageOutlined } from '@ant-design/icons';

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
      title={
        <Space size={8}>
          <MessageOutlined aria-hidden />
          השאלה
        </Space>
      }
      extra={isFetching ? <Tag color="processing">מעדכן…</Tag> : null}
    >
      {text ? (
        <Typography.Text>{text}</Typography.Text>
      ) : isFetching ? (
        <Skeleton active paragraph={false} />
      ) : null}
    </Card>
  );
}
