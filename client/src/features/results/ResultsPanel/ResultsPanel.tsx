import { Alert } from 'antd';
import { ApiError } from '../../../models/problemDetails';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { ResultsTable } from '../ResultsTable';

interface Props {
  response: SearchResponse | undefined;
  error: unknown;
  isFetching: boolean;
  registry: FilterFieldRegistryEntry[];
  definition: QueryDefinition;
  onPageChange: (pageNumber: number, pageSize: number) => void;
  onSortChange: (sort: SortSpec[]) => void;
}

/** Owns the results-area states: error banner, otherwise the table (which shows its own loading/empty). */
export function ResultsPanel({
  response,
  error,
  isFetching,
  registry,
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
        description={[problem?.detail, problem?.traceId && `traceId: ${problem.traceId}`]
          .filter(Boolean)
          .join(' · ')}
      />
    );
  }

  return (
    <ResultsTable
      response={response}
      registry={registry}
      definition={definition}
      loading={isFetching}
      onPageChange={onPageChange}
      onSortChange={onSortChange}
    />
  );
}
