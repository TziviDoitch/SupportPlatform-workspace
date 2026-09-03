import { fireEvent, render, screen } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { QueryDefinition } from '../../../models/queryDefinition';

const mutate = vi.fn();
vi.mock('../hooks/useSavedQueries', () => ({
  useCreateSavedQuery: () => ({ mutate, isPending: false }),
}));

const { SaveQueryButton } = await import('./SaveQueryButton');

const definition: QueryDefinition = {
  tenantId: 'culture-sport-admin',
  filters: { status: ['approved'] },
  segmentation: ['supportYear'],
  metrics: ['count'],
  paging: { pageNumber: 1, pageSize: 50 },
  sort: [],
};

describe('SaveQueryButton', () => {
  beforeEach(() => mutate.mockReset());

  it('posts the name and the current definition', () => {
    render(
      <ConfigProvider locale={heIL}>
        <SaveQueryButton definition={definition} />
      </ConfigProvider>,
    );

    fireEvent.click(screen.getByRole('button', { name: 'שמור שאילתה' }));
    fireEvent.change(screen.getByLabelText('שם שאילתה'), { target: { value: 'הבקשות שלי' } });
    fireEvent.click(screen.getByRole('button', { name: 'שמור' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(mutate.mock.calls[0][0]).toEqual({ name: 'הבקשות שלי', definition });
  });

  it('does not post when the name is blank', () => {
    render(
      <ConfigProvider locale={heIL}>
        <SaveQueryButton definition={definition} />
      </ConfigProvider>,
    );

    fireEvent.click(screen.getByRole('button', { name: 'שמור שאילתה' }));
    fireEvent.click(screen.getByRole('button', { name: 'שמור' }));

    expect(mutate).not.toHaveBeenCalled();
  });
});
