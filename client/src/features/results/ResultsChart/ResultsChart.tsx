import { Card } from 'antd';
import { BarChart } from '../../../components/BarChart';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { SearchResponse } from '../../../models/search';
import { buildChartData } from '../buildChartData';

interface Props {
  response: SearchResponse | undefined;
  segmentation: string[];
  registry: FilterFieldRegistryEntry[];
  references: References;
}

/**
 * Bar chart over the response `aggregations`, shown only when the query is segmented by exactly one
 * field (see {@link buildChartData}). Renders nothing otherwise — the table already covers those
 * cases. The chart tracks the segmentation because its data comes straight from the response.
 */
export function ResultsChart({ response, segmentation, registry, references }: Props) {
  const data = response && buildChartData(response.aggregations, segmentation, registry, references);
  if (!data) return null;

  return (
    <Card size="small" title="גרף" style={{ marginBottom: 16 }}>
      <BarChart labels={data.labels} values={data.values} seriesLabel={data.seriesLabel} />
    </Card>
  );
}
