# Contract — Error Model

> Frozen in S0-d. Source: `IMPLEMENTATION_PLAN.md` §5. The global exception
> handler / ProblemDetails middleware and the correlation id are implemented in
> S2; this file is the shape they must emit.

Every error response is [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807)
`ProblemDetails`.

- `Content-Type: application/problem+json`
- HTTP status = the `status` field.

## Shape

```json
{
  "type": "https://supportplatform.local/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The query definition failed validation.",
  "traceId": "0HN...:00000001",
  "errors": {
    "filters.supportYear": ["'from' must be less than or equal to 'to'."],
    "segmentation[0]": ["'costCenter' is not a known filter field."]
  }
}
```

| Field | Always | Meaning |
|---|---|---|
| `type` | yes | Stable URI identifying the error class. Opaque — for docs/grouping, not for parsing. |
| `title` | yes | Short, stable, human-readable summary of the `type`. |
| `status` | yes | HTTP status code, mirrored in the response line. |
| `detail` | yes | Human-readable explanation specific to this occurrence. |
| `traceId` | yes | Request **correlation id** — the same value emitted on every Serilog line for the request (§2). Quote it in bug reports. |
| `errors` | 400 only | Map of `field path` → `string[]` messages, from FluentValidation on the `QueryDefinition`. Field paths use dot/bracket notation matching the request body. |

## Status catalogue

| Status | `type` slug | When |
|---|---|---|
| `400` | `validation` | Body fails schema or FluentValidation: unknown field id, filter value shape ≠ registry `kind`, operator not allowed, reversed year range, bad enum (`metrics`, `sort.direction`), `pageSize` out of 1–200, missing `tenantId` on `/api/metadata`. |
| `401` | `unauthorized` | **Production target — not emitted by the PoC.** No token / `X-User`, or an invalid/expired token. The PoC has no auth middleware; a missing/unknown `X-User` resolves to a seed user (`ARCHITECTURE.md` §8.1), so this status never occurs. |
| `403` | `forbidden` | Authenticated but out of scope: `tenantId` in the body ≠ the caller's tenant, or a role lacking the required permission. |
| `404` | `not-found` | Route has no match, or a saved query is not in the caller's owner+tenant scope (existence is not leaked — see `api-contract.md` §5). |
| `500` | `unexpected` | Unhandled exception. `detail` is generic; the real cause is in the logs under `traceId`. |

## Examples

**401** _(production target — the PoC never emits this)_

```json
{
  "type": "https://supportplatform.local/errors/unauthorized",
  "title": "Authentication required.",
  "status": 401,
  "detail": "No bearer token or X-User header was supplied.",
  "traceId": "0HN...:00000007"
}
```

**403**

```json
{
  "type": "https://supportplatform.local/errors/forbidden",
  "title": "Access denied.",
  "status": 403,
  "detail": "tenantId 'welfare-admin' does not match the caller's tenant.",
  "traceId": "0HN...:00000009"
}
```

**404**

```json
{
  "type": "https://supportplatform.local/errors/not-found",
  "title": "Resource not found.",
  "status": 404,
  "detail": "Saved query 'b3f1c8e2-...' was not found.",
  "traceId": "0HN...:0000000c"
}
```

**500**

```json
{
  "type": "https://supportplatform.local/errors/unexpected",
  "title": "An unexpected error occurred.",
  "status": 500,
  "detail": "The request could not be completed. Reference traceId when reporting.",
  "traceId": "0HN...:0000000f"
}
```
