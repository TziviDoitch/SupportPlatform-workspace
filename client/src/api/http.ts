import { notification } from 'antd';
import { ApiError, type ProblemDetails } from '../models/problemDetails';

/**
 * The one HTTP seam. Every `src/api` service goes through here; components never call `fetch`.
 * On a non-2xx response it parses RFC 7807 ProblemDetails, surfaces it as a notification
 * (the "interceptor"), and throws {@link ApiError} so callers still see the failure.
 */
async function request<T>(method: 'GET' | 'POST', url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (!res.ok) {
    throw toApiError(await safeParseProblem(res));
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

function toApiError(problem: ProblemDetails): ApiError {
  const error = new ApiError(problem);
  // S7 (UI polish / RTL): switch to an antd `<App>` notification instance so it picks up theme + dir.
  notification.error({
    message: problem.title,
    description: [problem.detail, problem.traceId && `traceId: ${problem.traceId}`]
      .filter(Boolean)
      .join(' · '),
  });
  return error;
}

export const http = {
  get: <T>(url: string) => request<T>('GET', url),
  post: <T>(url: string, body: unknown) => request<T>('POST', url, body),
};
