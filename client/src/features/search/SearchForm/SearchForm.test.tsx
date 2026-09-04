import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import { emptyFormState } from '../buildQueryDefinition';
import { SearchForm } from './SearchForm';

const registry: FilterFieldRegistryEntry[] = [
  { id: 'bodyType', label: 'סוג גוף', kind: 'codeList', referenceList: 'bodyTypes', operators: ['in'], segmentable: true },
  { id: 'status', label: 'סטטוס', kind: 'codeList', referenceList: 'statuses', operators: ['in'], segmentable: false },
  { id: 'supportYear', label: 'שנת תמיכה', kind: 'yearRange', operators: ['range', 'single'], segmentable: true },
];

const references: References = {
  domains: [],
  bodyTypes: [{ code: 'association', label: 'עמותה' }],
  statuses: [{ code: 'approved', label: 'מאושר' }],
  districts: [],
};

function renderForm(overrides: Partial<Parameters<typeof SearchForm>[0]> = {}) {
  const props = {
    registry,
    references,
    state: emptyFormState,
    onFieldChange: vi.fn(),
    onGraphFieldsChange: vi.fn(),
    onSearch: vi.fn(),
    onClear: vi.fn(),
    ...overrides,
  };
  render(<SearchForm {...props} />);
  return props;
}

describe('SearchForm', () => {
  it('renders one labelled control per registry entry, in order, plus the graph-fields picker', async () => {
    renderForm();
    for (const label of ['סוג גוף', 'סטטוס', 'שנת תמיכה', 'הוספת גרף לפי']) {
      expect(screen.getByText(label)).toBeTruthy();
    }
    // two code-list selects + from/to year selects + the graph-fields select
    expect(await screen.findAllByRole('combobox')).toHaveLength(5);
    // year range is a dropdown now — no free-form number inputs
    expect(screen.queryAllByRole('spinbutton')).toHaveLength(0);
  });

  it('reflects the current selection from state', () => {
    renderForm({ state: { ...emptyFormState, values: { bodyType: ['association'] } } });
    expect(screen.getByText('עמותה')).toBeTruthy();
  });

  it('has no control for a field that is not in the registry', () => {
    renderForm();
    expect(screen.queryByText('מחוז')).toBeNull();
  });

  it('runs the search on submit and clears on the clear action', () => {
    const props = renderForm();
    fireEvent.click(screen.getByRole('button', { name: 'חיפוש' }));
    expect(props.onSearch).toHaveBeenCalledOnce();
    fireEvent.click(screen.getByRole('button', { name: /ניקוי מאפייני חיפוש/ }));
    expect(props.onClear).toHaveBeenCalledOnce();
  });

  it('toggles the filter panel open and closed', () => {
    const { container } = render(
      <SearchForm
        registry={registry}
        references={references}
        state={emptyFormState}
        onFieldChange={vi.fn()}
        onGraphFieldsChange={vi.fn()}
        onSearch={vi.fn()}
        onClear={vi.fn()}
      />,
    );
    const body = container.querySelector('.ant-card-body') as HTMLElement;
    expect(body.style.display).toBe('');
    fireEvent.click(screen.getByRole('button', { name: /הסתרת מאפייני חיפוש/ }));
    expect(body.style.display).toBe('none');
    expect(screen.getByRole('button', { name: /הצגת מאפייני חיפוש/ })).toBeTruthy();
  });
});
