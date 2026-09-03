import { Alert, Spin } from 'antd';
import { DEFAULT_TENANT_ID } from '../../../api/config';
import type { MetadataResponse } from '../../../models/metadata';
import { QuestionPanel } from '../../results/QuestionPanel';
import { ResultsPanel } from '../../results/ResultsPanel';
import { useSearch } from '../../results/hooks/useSearch';
import { SearchForm } from '../SearchForm';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { useMetadata } from '../hooks/useMetadata';
import { useSearchForm } from '../hooks/useSearchForm';

const SEARCH_DEBOUNCE_MS = 400;

/** The S3 vertical slice: metadata → dynamic form → QueryDefinition → /api/search → question + table. */
export function SearchPage() {
  const { data: metadata, isLoading, error } = useMetadata(DEFAULT_TENANT_ID);

  if (isLoading) return <Spin />;
  if (error || !metadata) {
    return <Alert type="error" showIcon message="טעינת נתוני הסינון נכשלה" />;
  }
  return <SearchView metadata={metadata} />;
}

function SearchView({ metadata }: { metadata: MetadataResponse }) {
  const form = useSearchForm(metadata.filterFieldRegistry, DEFAULT_TENANT_ID);
  const definition = useDebouncedValue(form.definition, SEARCH_DEBOUNCE_MS);
  const { data, error, isFetching } = useSearch(definition);

  return (
    <>
      <SearchForm
        registry={metadata.filterFieldRegistry}
        references={metadata.references}
        state={form.state}
        onFieldChange={form.setFieldValue}
        onSegmentationChange={form.setSegmentation}
      />
      <QuestionPanel text={data?.questionText} isFetching={isFetching} />
      <ResultsPanel
        response={data}
        error={error}
        isFetching={isFetching}
        registry={metadata.filterFieldRegistry}
        definition={definition}
        onPageChange={form.setPage}
        onSortChange={form.setSort}
      />
    </>
  );
}
