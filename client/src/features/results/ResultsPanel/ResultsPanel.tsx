import { Alert, Col, Row } from 'antd';
import { ApiError, formatProblemDetail } from '../../../models/problemDetails';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { buildChartData } from '../buildChartData';
import { ResultsChart } from '../ResultsChart';
import { ResultsTable } from '../ResultsTable';

interface Props {
  response: SearchResponse | undefined;
  error: unknown;
  isFetching: boolean;
  registry: FilterFieldRegistryEntry[];
  references: References;
  definition: QueryDefinition;
  /** Omit both to render a read-only table (no pager, non-sortable headers) — e.g. the saved-query re-run. */
  onPageChange?: (pageNumber: number, pageSize: number) => void;
  onSortChange?: (sort: SortSpec[]) => void;
}

/** Owns the results-area states: error banner, otherwise the table with the chart beside it (when segmented). */
export function ResultsPanel({
  response,
  error,
  isFetching,
  registry,
  references,
  definition,
  onPageChange,
  onSortChange,
}: Props) {
  if (error) {
    const problem = error instanceof ApiError ? error : undefined;
    return (
      <Alert
        type="error"
        showIcon
        message={problem?.title ?? 'החיפוש נכשל'}
        description={problem ? formatProblemDetail(problem) : undefined}
      />
    );
  }

  const chart = response
    ? buildChartData(response.aggregations, definition.segmentation, registry, references)
    : null;

  return (
    <Row gutter={[16, 16]} align="stretch">
      <Col xs={24} xl={chart ? 15 : 24}>
        <ResultsTable
          response={response}
          registry={registry}
          definition={definition}
          loading={isFetching}
          onPageChange={onPageChange}
          onSortChange={onSortChange}
        />
      </Col>
      {chart && (
        <Col xs={24} xl={9}>
          <ResultsChart data={chart} />
        </Col>
      )}
    </Row>
  );
}
