import { Card } from 'antd';
import { BarChartOutlined } from '@ant-design/icons';
import { BarChart } from '../../../components/BarChart';
import { SectionTitle } from '../../../components/SectionTitle';
import type { ChartData } from '../buildChartData';

interface Props {
  data: ChartData;
}

export const ResultsChart = ({ data }: Props) => {
  return (
    <Card
      title={<SectionTitle icon={<BarChartOutlined />}>{data.seriesLabel}</SectionTitle>}
      style={{ height: '100%' }}
    >
      <BarChart labels={data.labels} values={data.values} seriesLabel={data.seriesLabel} />
    </Card>
  );
};
