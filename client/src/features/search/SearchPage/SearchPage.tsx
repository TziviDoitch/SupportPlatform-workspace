import { Alert, Card, Typography } from 'antd';
import { DEFAULT_TENANT_ID } from '../../../api/config';
import { PageLoader } from '../../../components/PageLoader';
import type { MetadataResponse } from '../../../models/metadata';
import { SaveQueryButton } from '../../saved-queries/SaveQueryButton';
import { QuestionPanel } from '../../results/QuestionPanel';
import { ResultsPanel } from '../../results/ResultsPanel';
import { useSearch } from '../../results/hooks/useSearch';
import { SearchForm } from '../SearchForm';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { useMetadata } from '../../../hooks/useMetadata';
import { useSearchForm } from '../hooks/useSearchForm';

const SEARCH_DEBOUNCE_MS = 400;

/** The S3 vertical slice: metadata → dynamic form → QueryDefinition → /api/search → question + table. */
export function SearchPage() {
  const { data: metadata, isLoading, error } = useMetadata(DEFAULT_TENANT_ID);

  return (
    <Card size="small">
      <Typography.Title level={4} style={{ marginTop: 0 }}>
        חיפוש
      </Typography.Title>
      {isLoading ? (
        <PageLoader />
      ) : error || !metadata ? (
        <Alert type="error" showIcon message="טעינת נתוני הסינון נכשלה" />
      ) : (
        <SearchView metadata={metadata} />
      )}
    </Card>
  );
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
      <div style={{ marginBottom: 16 }}>
        <SaveQueryButton definition={form.definition} />
      </div>
      <QuestionPanel text={data?.questionText} isFetching={isFetching} />
      <ResultsPanel
        response={data}
        error={error}
        isFetching={isFetching}
        registry={metadata.filterFieldRegistry}
        references={metadata.references}
        definition={definition}
        onPageChange={form.setPage}
        onSortChange={form.setSort}
      />
    </>
  );
}
