import { Card } from 'antd';
import { BarChartOutlined } from '@ant-design/icons';
import { BarChart } from '../../../components/BarChart';
import { SectionTitle } from '../../../components/SectionTitle';
import type { ChartData } from '../buildChartData';

interface Props {
  /** One field's series from {@link buildCharts}; the parent renders one card per entry. */
  data: ChartData;
}

/** Card-framed bar chart for a single segmentation field. Sits beside the results table. */
export function ResultsChart({ data }: Props) {
  return (
    <Card
      title={<SectionTitle icon={<BarChartOutlined />}>{data.seriesLabel}</SectionTitle>}
      style={{ height: '100%' }}
    >
      <BarChart labels={data.labels} values={data.values} seriesLabel={data.seriesLabel} />
    </Card>
  );
}
