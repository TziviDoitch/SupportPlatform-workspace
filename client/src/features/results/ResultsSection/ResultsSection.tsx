import { Space } from 'antd';
import { PageLoader } from '../../../components/PageLoader';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import type { SearchResponse } from '../../../models/search';
import { QuestionPanel } from '../QuestionPanel';
import { ResultsPanel } from '../ResultsPanel';

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

export const ResultsSection = ({ response, error, isFetching, ...rest }: Props) => {
  return (
    <Space direction="vertical" size={16} style={{ display: 'flex' }}>
      <QuestionPanel text={response?.questionText} isFetching={isFetching} />
      {error || response ? (
        <ResultsPanel response={response} error={error} isFetching={isFetching} {...rest} />
      ) : isFetching ? (
        <PageLoader size="large" />
      ) : null}
    </Space>
  );
};
