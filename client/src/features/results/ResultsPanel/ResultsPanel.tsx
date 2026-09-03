import { Alert } from 'antd';
import { ApiError, formatProblemDetail } from '../../../models/problemDetails';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
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

/** Owns the results-area states: error banner, otherwise the chart (when segmented) + the table. */
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

  return (
    <>
      <ResultsChart
        response={response}
        segmentation={definition.segmentation}
        registry={registry}
        references={references}
      />
      <ResultsTable
        response={response}
        registry={registry}
        definition={definition}
        loading={isFetching}
        onPageChange={onPageChange}
        onSortChange={onSortChange}
      />
    </>
  );
}
