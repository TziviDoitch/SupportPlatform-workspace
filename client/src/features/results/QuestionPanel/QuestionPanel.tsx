import { Card, Skeleton, Tag, Typography } from 'antd';
import { MessageOutlined } from '@ant-design/icons';
import { SectionTitle } from '../../../components/SectionTitle';

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
      title={<SectionTitle icon={<MessageOutlined />}>השאלה</SectionTitle>}
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
