import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
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

const thisYear = new Date().getFullYear();

/** Open an antd Select and wait for its option list (rendered in a body portal). */
async function open(combobox: HTMLElement): Promise<HTMLElement> {
  fireEvent.mouseDown(combobox);
  return waitFor(() => {
    const dropdown = document.querySelector(
      '.ant-select-dropdown:not(.ant-select-dropdown-hidden)',
    ) as HTMLElement | null;
    if (!dropdown || !dropdown.querySelector('.ant-select-item-option')) {
      throw new Error('dropdown not open yet');
    }
    return dropdown;
  });
}

describe('YearRangeField', () => {
  it('offers two year dropdowns and no free-form number input', () => {
    render(<YearRangeField entry={entry} value={{}} onChange={vi.fn()} />);
    expect(screen.getAllByRole('combobox')).toHaveLength(2);
    expect(screen.queryAllByRole('spinbutton')).toHaveLength(0);
  });

  it('emits the picked "from" year', async () => {
    const onChange = vi.fn();
    render(<YearRangeField entry={entry} value={{}} onChange={onChange} />);
    const dropdown = await open(screen.getByRole('combobox', { name: 'שנת תמיכה — משנה' }));
    fireEvent.click(dropdown.querySelector(`.ant-select-item-option[title="${thisYear}"]`)!);
    expect(onChange).toHaveBeenLastCalledWith({ from: thisYear });
  });

  it('reflects both ends of a controlled range', () => {
    render(<YearRangeField entry={entry} value={{ from: 2018, to: 2022 }} onChange={vi.fn()} />);
    const [from, to] = screen.getAllByRole('combobox');
    expect(within(from.closest('.ant-select')!).getByText('2018')).toBeTruthy();
    expect(within(to.closest('.ant-select')!).getByText('2022')).toBeTruthy();
  });

  it('hides "to" years before the chosen "from"', async () => {
    render(<YearRangeField entry={entry} value={{ from: thisYear }} onChange={vi.fn()} />);
    const dropdown = await open(screen.getByRole('combobox', { name: 'שנת תמיכה — עד שנה' }));
    expect(dropdown.querySelector(`.ant-select-item-option[title="${thisYear}"]`)).toBeTruthy();
    expect(dropdown.querySelector(`.ant-select-item-option[title="${thisYear - 1}"]`)).toBeNull();
  });
});
