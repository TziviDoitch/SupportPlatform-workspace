/** RFC 7807 ProblemDetails — the shape every API error uses (`docs/contracts/error-model.md`). */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  traceId?: string;
  /** Field-path → messages, present on 400 only. */
  errors?: Record<string, string[]>;
}

/** Thrown by the `src/api` layer for any non-2xx response. Carries the parsed ProblemDetails. */
export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly detail?: string;
  readonly traceId?: string;
  readonly errors?: Record<string, string[]>;

  constructor(problem: ProblemDetails) {
    super(problem.detail ?? problem.title);
    this.name = 'ApiError';
    this.status = problem.status;
    this.title = problem.title;
    this.detail = problem.detail;
    this.traceId = problem.traceId;
    this.errors = problem.errors;
  }
}
