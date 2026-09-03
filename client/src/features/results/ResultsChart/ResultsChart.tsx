import { Card, Space } from 'antd';
import { BarChartOutlined } from '@ant-design/icons';
import { BarChart } from '../../../components/BarChart';
import type { ChartData } from '../buildChartData';

interface Props {
  /** Pre-built series from {@link buildChartData}; the parent renders this only when it is non-null. */
  data: ChartData;
}

/** Card-framed bar chart over a response's aggregations. Sits beside the results table. */
export function ResultsChart({ data }: Props) {
  return (
    <Card
      title={
        <Space size={8}>
          <BarChartOutlined aria-hidden />
          גרף
        </Space>
      }
      style={{ height: '100%' }}
    >
      <BarChart labels={data.labels} values={data.values} seriesLabel={data.seriesLabel} />
    </Card>
  );
}
