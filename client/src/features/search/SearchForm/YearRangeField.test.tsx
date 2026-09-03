import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import { YearRangeField } from './YearRangeField';

const entry: FilterFieldRegistryEntry = {
  id: 'supportYear',
  label: 'שנת תמיכה',
  kind: 'yearRange',
  operators: ['range', 'single'],
  segmentable: true,
};

describe('YearRangeField', () => {
  it('bounds both inputs to a plausible calendar-year range', () => {
    render(<YearRangeField entry={entry} value={{}} onChange={vi.fn()} />);
    for (const input of screen.getAllByRole('spinbutton')) {
      expect(input.getAttribute('aria-valuemin')).toBe('2000');
      expect(input.getAttribute('aria-valuemax')).toBe('2100');
    }
  });

  it('emits the "from" year as it is typed', () => {
    const onChange = vi.fn();
    render(<YearRangeField entry={entry} value={{}} onChange={onChange} />);
    fireEvent.change(screen.getAllByRole('spinbutton')[0], { target: { value: '2023' } });
    expect(onChange).toHaveBeenLastCalledWith({ from: 2023 });
  });

  it('clears the filter when the only set end is emptied', () => {
    const onChange = vi.fn();
    render(<YearRangeField entry={entry} value={{ from: 2023 }} onChange={onChange} />);
    fireEvent.change(screen.getAllByRole('spinbutton')[0], { target: { value: '' } });
    expect(onChange).toHaveBeenLastCalledWith(undefined);
  });
});
