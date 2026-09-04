import { Alert, Col, Row } from 'antd';
import { ApiError, formatProblemDetail } from '../../../models/problemDetails';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { buildCharts } from '../buildChartData';
import { ResultsChart } from '../ResultsChart';
import { ResultsTable } from '../ResultsTable';
import { t } from '../../../i18n';

interface Props {
  response: SearchResponse | undefined;
  error: unknown;
  isFetching: boolean;
  registry: FilterFieldRegistryEntry[];
  references: References;
  definition: QueryDefinition;
  graphFields?: string[];
  onPageChange?: (pageNumber: number, pageSize: number) => void;
  onSortChange?: (sort: SortSpec[]) => void;
}

const spans = (chartCount: number): { table: number; chart: number } => {
  switch (chartCount) {
    case 0:
      return { table: 24, chart: 0 };
    case 1:
      return { table: 15, chart: 9 };
    case 2:
      return { table: 12, chart: 6 };
    case 3:
      return { table: 12, chart: 4 };
    default:
      return { table: 24, chart: 6 };
  }
};

export const ResultsPanel = ({
  response,
  error,
  isFetching,
  registry,
  references,
  definition,
  graphFields,
  onPageChange,
  onSortChange,
}: Props) => {
  if (error) {
    const problem = error instanceof ApiError ? error : undefined;
    return (
      <Alert
        type="error"
        showIcon
        message={problem?.title ?? t.results.searchError}
        description={problem ? formatProblemDetail(problem) : undefined}
      />
    );
  }

  const charts = response
    ? buildCharts(response.aggregations, graphFields ?? definition.segmentation, registry, references)
    : [];
  const span = spans(charts.length);

  return (
    <Row gutter={[16, 16]} align="stretch">
      <Col xs={24} xl={span.table}>
        <ResultsTable
          response={response}
          registry={registry}
          references={references}
          definition={definition}
          loading={isFetching}
          onPageChange={onPageChange}
          onSortChange={onSortChange}
        />
      </Col>
      {charts.map((chart) => (
        <Col key={chart.seriesLabel} xs={24} md={12} xl={span.chart}>
          <ResultsChart data={chart} />
        </Col>
      ))}
    </Row>
  );
};
