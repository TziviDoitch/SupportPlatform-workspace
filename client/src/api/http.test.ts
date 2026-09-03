import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../models/problemDetails';

const notifyError = vi.fn();
vi.mock('antd', () => ({ notification: { error: (args: unknown) => notifyError(args) } }));

const { http } = await import('./http');

function mockFetch(status: number, body: unknown, ok = status < 400) {
  vi.stubGlobal(
    'fetch',
    vi.fn(async () => ({
      ok,
      status,
      statusText: 'x',
      json: async () => body,
    })),
  );
}

afterEach(() => {
  vi.unstubAllGlobals();
  notifyError.mockReset();
});

describe('http', () => {
  it('returns the parsed JSON on success', async () => {
    mockFetch(200, { hello: 'world' });
    await expect(http.get<{ hello: string }>('/api/x')).resolves.toEqual({ hello: 'world' });
    expect(notifyError).not.toHaveBeenCalled();
  });

  it('throws ApiError carrying the ProblemDetails and raises one notification', async () => {
    mockFetch(400, {
      type: 'https://supportplatform.local/errors/validation',
      title: 'One or more validation errors occurred.',
      status: 400,
      detail: 'The query definition failed validation.',
      traceId: '0HN-abc',
      errors: { 'filters.supportYear': ["'from' must be <= 'to'."] },
    });

    const err = (await http.post('/api/search', {}).catch((e) => e)) as ApiError;
    expect(err).toBeInstanceOf(ApiError);
    expect(err.status).toBe(400);
    expect(err.title).toBe('One or more validation errors occurred.');
    expect(err.traceId).toBe('0HN-abc');
    expect(err.errors).toEqual({ 'filters.supportYear': ["'from' must be <= 'to'."] });

    expect(notifyError).toHaveBeenCalledTimes(1);
    expect(notifyError.mock.calls[0][0].description).toContain('0HN-abc');
  });

  it('still throws ApiError when the error body is not ProblemDetails', async () => {
    mockFetch(500, 'boom');
    const err = (await http.get('/api/x').catch((e) => e)) as ApiError;
    expect(err).toBeInstanceOf(ApiError);
    expect(err.status).toBe(500);
  });
});
