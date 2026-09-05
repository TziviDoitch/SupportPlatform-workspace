import { useCallback, useMemo, useState } from 'react';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { QueryDefinition } from '../../../models/queryDefinition';
import {
  buildQueryDefinition,
  emptyFormState,
  type FieldValue,
  type SearchFormState,
} from '../buildQueryDefinition';

export interface SearchForm {
  state: SearchFormState;
  definition: QueryDefinition;
  setFieldValue: (fieldId: string, value: FieldValue | undefined) => void;
  setGraphFields: (ids: string[]) => void;
  reset: () => void;
}

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

  const setGraphFields = useCallback((ids: string[]) => {
    setState((s) => ({ ...s, graphFields: ids, pageNumber: 1 }));
  }, []);

  const reset = useCallback(() => setState(emptyFormState), []);

  const definition = useMemo(
    () => buildQueryDefinition(state, registry, tenantId),
    [state, registry, tenantId],
  );

  return { state, definition, setFieldValue, setGraphFields, reset };
}
