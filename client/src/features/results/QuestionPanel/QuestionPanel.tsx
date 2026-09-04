import { Card, Skeleton, Tag, Typography } from 'antd';
import { MessageOutlined } from '@ant-design/icons';
import { SectionTitle } from '../../../components/SectionTitle';
import { t } from '../../../i18n';

interface Props {
  text: string | undefined;
  isFetching: boolean;
}

export const QuestionPanel = ({ text, isFetching }: Props) => {
  return (
    <Card
      size="small"
      title={<SectionTitle icon={<MessageOutlined />}>{t.results.questionTitle}</SectionTitle>}
      extra={isFetching ? <Tag color="processing">{t.results.updating}</Tag> : null}
    >
      {text ? (
        <Typography.Text>{text}</Typography.Text>
      ) : isFetching ? (
        <Skeleton active paragraph={false} />
      ) : null}
    </Card>
  );
};
