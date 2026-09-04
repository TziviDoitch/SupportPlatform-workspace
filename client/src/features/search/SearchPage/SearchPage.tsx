import { useState } from 'react';
import { Alert, Card, Space, Typography } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { getActiveUser } from '../../../api/activeUser';
import { PageLoader } from '../../../components/PageLoader';
import { SectionTitle } from '../../../components/SectionTitle';
import type { MetadataResponse } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import { withPaging, withSort } from '../../../lib/queryDefinition';
import { SaveQueryButton } from '../../saved-queries/SaveQueryButton';
import { ResultsSection } from '../../results/ResultsSection';
import { useSearch } from '../../results/hooks/useSearch';
import { SearchForm } from '../SearchForm';
import { useMetadata } from '../../../hooks/useMetadata';
import { useSearchForm } from '../hooks/useSearchForm';
import { t } from '../../../i18n';

export const SearchPage = () => {
  const { data: metadata, isLoading, error } = useMetadata(getActiveUser().tenantId);

  return (
    <Space direction="vertical" size={20} style={{ display: 'flex' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        <SectionTitle icon={<SearchOutlined />}>{t.search.title}</SectionTitle>
      </Typography.Title>

      {isLoading ? (
        <Card>
          <PageLoader />
        </Card>
      ) : error || !metadata ? (
        <Alert type="error" showIcon message={t.search.filterError} />
      ) : (
        <SearchView metadata={metadata} />
      )}
    </Space>
  );
};

const SearchView = ({ metadata }: { metadata: MetadataResponse }) => {
  const form = useSearchForm(metadata.filterFieldRegistry, getActiveUser().tenantId);

  const [submitted, setSubmitted] = useState<{ definition: QueryDefinition; graphFields: string[] }>();
  const { data, error, isFetching } = useSearch(submitted?.definition);

  const runSearch = () =>
    setSubmitted({ definition: form.definition, graphFields: form.state.graphFields });
  const clear = () => {
    form.reset();
    setSubmitted(undefined);
  };
  const patch = (fn: (d: QueryDefinition) => QueryDefinition) =>
    setSubmitted((s) => s && { ...s, definition: fn(s.definition) });
  const setPage = (pageNumber: number, pageSize: number) =>
    patch((d) => withPaging(d, pageNumber, pageSize));
  const setSort = (sort: SortSpec[]) => patch((d) => withSort(d, sort));

  return (
    <>
      <SearchForm
        registry={metadata.filterFieldRegistry}
        references={metadata.references}
        state={form.state}
        isSearching={isFetching}
        onFieldChange={form.setFieldValue}
        onGraphFieldsChange={form.setGraphFields}
        onSearch={runSearch}
        onClear={clear}
      />

      {submitted && (
        <Space direction="vertical" size={16} style={{ display: 'flex' }}>
          <ResultsSection
            response={data}
            error={error}
            isFetching={isFetching}
            registry={metadata.filterFieldRegistry}
            references={metadata.references}
            definition={submitted.definition}
            graphFields={submitted.graphFields}
            onPageChange={setPage}
            onSortChange={setSort}
          />
          <SaveQueryButton definition={submitted.definition} />
        </Space>
      )}
    </>
  );
};
