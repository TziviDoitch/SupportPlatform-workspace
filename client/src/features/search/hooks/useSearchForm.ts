import { useCallback, useMemo, useState } from 'react';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { QueryDefinition, SortSpec } from '../../../models/queryDefinition';
import {
  buildQueryDefinition,
  emptyFormState,
  type FieldValue,
  type SearchFormState,
} from '../buildQueryDefinition';

export interface SearchForm {
  state: SearchFormState;
  /** Live definition rebuilt on every edit. */
  definition: QueryDefinition;
  setFieldValue: (fieldId: string, value: FieldValue | undefined) => void;
  setSegmentation: (ids: string[]) => void;
  setPage: (pageNumber: number, pageSize: number) => void;
  setSort: (sort: SortSpec[]) => void;
}

/** Holds the search-form state and rebuilds the canonical {@link QueryDefinition} from it. */
export function useSearchForm(registry: FilterFieldRegistryEntry[], tenantId: string): SearchForm {
  const [state, setState] = useState<SearchFormState>(emptyFormState);

  const setFieldValue = useCallback((fieldId: string, value: FieldValue | undefined) => {
    setState((s) => {
      const values = { ...s.values };
      if (value === undefined) delete values[fieldId];
      else values[fieldId] = value;
      return { ...s, values, pageNumber: 1 };
    });
  }, []);

  const setSegmentation = useCallback((ids: string[]) => {
    setState((s) => ({ ...s, segmentation: ids, sort: [], pageNumber: 1 }));
  }, []);

  const setPage = useCallback((pageNumber: number, pageSize: number) => {
    setState((s) => ({ ...s, pageNumber, pageSize }));
  }, []);

  const setSort = useCallback((sort: SortSpec[]) => {
    setState((s) => ({ ...s, sort, pageNumber: 1 }));
  }, []);

  const definition = useMemo(
    () => buildQueryDefinition(state, registry, tenantId),
    [state, registry, tenantId],
  );

  return { state, definition, setFieldValue, setSegmentation, setPage, setSort };
}
