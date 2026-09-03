import { ApiError, formatProblemDetail, type ProblemDetails } from '../models/problemDetails';
import { DEFAULT_USER } from './config';
import { notifyError } from './notificationHost';

type Method = 'GET' | 'POST' | 'PUT' | 'DELETE';

interface RequestOptions {
  /**
   * Raise the error toast on a non-2xx response. Default `true`. Pass `false` for a call whose
   * screen already shows the failure inline (e.g. the search results area), so the same error
   * isn't surfaced twice.
   */
  notify?: boolean;
}

/**
 * The one HTTP seam. Every `src/api` service goes through here; components never call `fetch`.
 * On a non-2xx response it parses RFC 7807 ProblemDetails, surfaces it as a notification
 * (the "interceptor") unless `notify: false`, and throws {@link ApiError} so callers still see
 * the failure.
 */
async function request<T>(
  method: Method,
  url: string,
  body?: unknown,
  opts?: RequestOptions,
): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: {
      'X-User': DEFAULT_USER,
      ...(body === undefined ? {} : { 'Content-Type': 'application/json' }),
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (!res.ok) {
    throw toApiError(await safeParseProblem(res), opts?.notify ?? true);
  }

  if (res.status === 204) {
    return undefined as T;
  }
  return (await res.json()) as T;
}

async function safeParseProblem(res: Response): Promise<ProblemDetails> {
  try {
    const parsed = (await res.json()) as Partial<ProblemDetails>;
    if (parsed && typeof parsed.title === 'string') {
      return { status: res.status, ...parsed } as ProblemDetails;
    }
  } catch {
    // fall through to a synthetic problem
  }
  return { type: 'about:blank', title: res.statusText || 'Request failed', status: res.status };
}

function toApiError(problem: ProblemDetails, notify: boolean): ApiError {
  const error = new ApiError(problem);
  if (notify) {
    // Routed through `notificationHost` so the toast is raised by the antd `<App>` instance
    // (theme + RTL aware); it falls back to the static API outside the UI tree.
    notifyError({ message: problem.title, description: formatProblemDetail(problem) });
  }
  return error;
}

export const http = {
  get: <T>(url: string, opts?: RequestOptions) => request<T>('GET', url, undefined, opts),
  post: <T>(url: string, body?: unknown, opts?: RequestOptions) => request<T>('POST', url, body, opts),
  put: <T>(url: string, body: unknown, opts?: RequestOptions) => request<T>('PUT', url, body, opts),
  del: <T>(url: string, opts?: RequestOptions) => request<T>('DELETE', url, undefined, opts),
};
