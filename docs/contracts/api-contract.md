# Contract — REST API

> Frozen in S0-d. Source: `IMPLEMENTATION_PLAN.md` §5. Once S2 lands, Swagger at
> `/swagger` is the live contract; this file is the frozen intent it must match.

- Base path: `/api`.
- All request and response bodies are JSON (`application/json`).
- All errors are RFC 7807 ProblemDetails — see [`error-model.md`](error-model.md).
- Auth: `Authorization: Bearer <jwt>` from `/api/auth/login`. Fallback for the PoC
  (§2, §7.6): an `X-User: <username>` header instead of a JWT. Either way the
  server derives `tenantId` and role from the caller — a mismatching `tenantId`
  in the body is a 403.
- **PoC status:** `/api/auth/login`, JWT bearer auth, and every `401` below are the
  **production target and are not implemented**. The PoC ships only the `X-User`
  seam; a missing/unknown header resolves to a seed user, so no request ever
  returns `401` (`ARCHITECTURE.md` §8.1). All other rows match the running server.

## Endpoints

| # | Method | Path | Auth | Purpose |
|---|---|---|---|---|
| 1 | POST | `/api/auth/login` | no | Exchange seed credentials for a token |
| 2 | GET | `/api/metadata?tenantId=` | yes | Reference lists + filter field registry (feeds the dynamic form) |
| 3 | POST | `/api/search` | yes | Run a `QueryDefinition`, get rows + aggregations + question text |
| 4 | POST | `/api/nl-queries/parse` | yes | Free text → `QueryDefinition` + interpretation |
| 5 | GET/POST/PUT/DELETE | `/api/saved-queries[/{id}]` | yes | CRUD over saved queries, scoped to owner + tenant |
| 6 | POST | `/api/saved-queries/{id}/run` | yes | Re-run a saved query; response identical to `/api/search` |

---

### 1. `POST /api/auth/login`

> **Not implemented in the PoC** — documented as the production target. The PoC
> authenticates via the `X-User` header only (see the Auth note above).

Request:

```json
{ "username": "sarah", "password": "pass" }
```

Response `200`:

```json
{
  "token": "<jwt>",
  "user": { "username": "sarah", "tenantId": "culture-sport-admin", "role": "analyst" }
}
```

Errors: `400` malformed body, `401` bad credentials.

---

### 2. `GET /api/metadata?tenantId=culture-sport-admin`

No request body. Response `200`: reference lists + `filterFieldRegistry`. Full
shape in [`metadata-model.md`](metadata-model.md).

Errors: `400` missing `tenantId`, `401` unauthenticated, `403` `tenantId` not the
caller's tenant.

---

### 3. `POST /api/search`

Request body: a [`QueryDefinition`](query-definition.md).

Response `200`:

```json
{
  "questionText": "כמה בקשות תמיכה ...",
  "rows": [ { "supportYear": 2023, "count": 12 } ],
  "aggregations": [
    { "key": { "supportYear": 2023 }, "metrics": { "count": 12 } }
  ],
  "page": { "pageNumber": 1, "pageSize": 50, "totalGroups": 3 },
  "executionMeta": {
    "durationMs": 41,
    "rowCount": 3,
    "cacheHit": false,
    "definitionHash": "sha256:9f2b..."
  }
}
```

- `questionText` — Hebrew sentence from `QuestionTextRenderer` (S2).
- `rows` — **the requested page** of result rows (shape depends on `segmentation` +
  `metrics`). Bounded by `paging.pageSize`.
- `aggregations` — **every** `segmentation` group, not just the page: `key` echoes the
  grouped field ids, `metrics` holds the requested metric values. No `segmentation` → a
  single entry with an empty `key`. This is the field to sum for totals and to plot —
  summing `rows` would describe only the page the client happens to show.
- `page` — echoes `paging` plus `totalGroups` (number of groups before paging;
  `aggregations.length` equals it).
- `executionMeta.rowCount` — how many rows were actually returned, i.e. `rows.length`.
- `executionMeta.definitionHash` — canonical SHA-256 of the definition (S5),
  drives `cacheHit`.

Errors: `400` invalid definition (unknown field id, reversed range, bad enum),
`401`, `403` tenant mismatch.

---

### 4. `POST /api/nl-queries/parse`

Request:

```json
{ "text": "כמה עמותות בתחום התרבות אושרו בשנת 2024", "tenantId": "culture-sport-admin" }
```

Response `200`:

```json
{
  "definition": { "...": "a QueryDefinition" },
  "interpretationText": "חיפוש בקשות ... בתחום תרבות ... סטטוס מאושר ... שנת 2024",
  "confidence": 0.82,
  "unresolved": ["district"]
}
```

- `definition` — a `QueryDefinition` the client can review, run, or save.
- `interpretationText` — human-readable read-back of what was understood.
- `confidence` — `0..1`.
- `unresolved` — phrases/fields the translator could not map (may be empty).

Errors: `400` empty `text`, `401`, `403` tenant mismatch.

---

### 5. `/api/saved-queries`

A saved query record:

```json
{
  "id": "b3f1c8e2-...",
  "name": "Approved culture associations 2023-2025",
  "definition": { "...": "a QueryDefinition" },
  "ownerUsername": "sarah",
  "tenantId": "culture-sport-admin",
  "createdAt": "2025-01-10T08:00:00Z",
  "lastRunAt": "2025-01-12T14:22:00Z",
  "lastRunRowCount": 3
}
```

| Method | Path | Body in | Response |
|---|---|---|---|
| GET | `/api/saved-queries` | – | `200` — array of records for the caller (own + tenant scope) |
| GET | `/api/saved-queries/{id}` | – | `200` — one record; `404` if not found in scope |
| POST | `/api/saved-queries` | `{ "name": "...", "definition": { } }` | `201` — created record |
| PUT | `/api/saved-queries/{id}` | `{ "name": "...", "definition": { } }` | `200` — updated record; `404` out of scope |
| DELETE | `/api/saved-queries/{id}` | – | `204`; `404` out of scope |

Scoping: a caller sees and mutates only queries they own within their tenant.
Acting on another user's query → `404` (not `403`, to avoid leaking existence).
`definition` is validated exactly like `/api/search` on POST/PUT.

Errors: `400` invalid `name`/`definition`, `401`, `404` out of scope.

---

### 6. `POST /api/saved-queries/{id}/run`

No request body. Runs the stored `definition`; response is identical to
`/api/search`. Side effect: updates `lastRunAt` and `lastRunRowCount`.

Errors: `401`, `404` out of scope.
