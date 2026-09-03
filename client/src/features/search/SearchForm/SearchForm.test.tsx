import { render, screen } from '@testing-library/react';
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

function renderForm(state = emptyFormState) {
  return render(
    <SearchForm
      registry={registry}
      references={references}
      state={state}
      onFieldChange={vi.fn()}
      onSegmentationChange={vi.fn()}
    />,
  );
}

describe('SearchForm', () => {
  it('renders one labelled control per registry entry, in order, plus a segmentation picker', () => {
    renderForm();
    for (const label of ['סוג גוף', 'סטטוס', 'שנת תמיכה', 'פילוח']) {
      expect(screen.getByText(label)).toBeTruthy();
    }
    // two code-list selects + the segmentation select
    expect(screen.getAllByRole('combobox')).toHaveLength(3);
    // year range → from/to number inputs
    expect(screen.getAllByRole('spinbutton')).toHaveLength(2);
  });

  it('reflects the current selection from state', () => {
    renderForm({ ...emptyFormState, values: { bodyType: ['association'] } });
    expect(screen.getByText('עמותה')).toBeTruthy();
  });

  it('has no control for a field that is not in the registry', () => {
    renderForm();
    expect(screen.queryByText('מחוז')).toBeNull();
  });
});
