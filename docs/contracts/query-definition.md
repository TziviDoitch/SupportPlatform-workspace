# Contract — `QueryDefinition`

> Frozen in S0-d. Source: `IMPLEMENTATION_PLAN.md` §5. Change only by an explicit
> plan revision — every later stage attaches this file to its task card.

`QueryDefinition` is the **single canonical object** in the system:

- the dynamic search form **builds** it,
- the NL parser (`/api/nl-queries/parse`) **emits** it,
- a saved query **is** one (stored verbatim),
- `DynamicQueryBuilder` **translates** it to a safe `IQueryable`,
- `QuestionTextRenderer` **reads** it to produce the Hebrew question sentence.

Everything downstream depends on this shape. Keep it small.

## Shape

```jsonc
{
  "tenantId": "culture-sport-admin",       // string, required

  "filters": {                             // object, required (may be empty {})
    "bodyType":     ["association"],        // string[] — codes, IN semantics
    "supportDomain": ["culture"],
    "status":       ["approved"],
    "district":     ["north"],
    "supportYear":  { "type": "range", "from": 2023, "to": 2025 }
    //              or { "type": "single", "value": 2025 }
  },

  "segmentation": ["supportYear"],          // string[] — subset of segmentable field ids
  "metrics":      ["count"],                // string[] — "count" | "sumAmountApproved"
  "paging":       { "pageSize": 50, "pageNumber": 1 },
  "sort":         [ { "field": "supportYear", "direction": "asc" } ]
}
```

## Fields

| Field | Type | Required | Notes |
|---|---|---|---|
| `tenantId` | string | yes | The organization scope. Must be a known tenant; the server also enforces it from the caller's identity. |
| `filters` | object | yes | Keys are **canonical field ids** (see below). Empty `{}` means "no filter". Every key **must** exist in `filter_field_registry` — anything else is rejected (400). |
| `filters[fieldId]` — code list | `string[]` | — | One or more reference **codes**. Multiple values = `IN`. Empty array is invalid — omit the key instead. |
| `filters[fieldId]` — year, range | `{ "type": "range", "from": int, "to": int }` | — | Inclusive. `from <= to` (reversed range → 400). |
| `filters[fieldId]` — year, single | `{ "type": "single", "value": int }` | — | Exact year. |
| `segmentation` | `string[]` | no | Group results by these field ids. Each must be a registry field with `segmentable: true`. Order is the grouping order. Empty/omitted = no grouping (single total row). |
| `metrics` | `string[]` | no | `count` (Must) and/or `sumAmountApproved` (Should). Omitted → `["count"]`. |
| `paging` | `{ pageSize: int, pageNumber: int }` | no | `pageSize` 1–200, `pageNumber` ≥ 1. Omitted → `{ pageSize: 50, pageNumber: 1 }`. |
| `sort` | `[{ field, direction }]` | no | `field` is a canonical field id; `direction` is `asc` or `desc`. Omitted → server default order. |

## Canonical field ids

Field ids are defined **once** in `filter_field_registry` (see
[`metadata-model.md`](metadata-model.md)) and used identically as `filters` keys,
`segmentation` entries, and `sort[].field`.

| Field id | Kind | Reference list | Segmentable |
|---|---|---|---|
| `bodyType` | codeList | `bodyTypes` | yes |
| `supportDomain` | codeList | `domains` | yes |
| `status` | codeList | `statuses` | no |
| `district` | codeList | `districts` | yes |
| `supportYear` | yearRange | — | yes |

> **Naming note.** This contract normalizes the §5 draft: `submittingBodyType` →
> `bodyType`, and the year field is `supportYear` everywhere (the draft used
> `year` as a filter key and `supportYear` in `sort`). The registry is the single
> source of these ids.

## Whitelist rule (red line — §3.4)

`DynamicQueryBuilder` builds the `IQueryable` **only** from field ids present in
`filter_field_registry`. A `filters` key, `segmentation` entry, or `sort[].field`
that is not in the registry is **rejected with 400** — it is never passed to the
query. Operators are also constrained by the registry entry (`in` for code lists,
`range` / `single` for `supportYear`).

## Worked example

```json
{
  "tenantId": "culture-sport-admin",
  "filters": {
    "bodyType": ["association"],
    "supportDomain": ["culture"],
    "status": ["approved"],
    "supportYear": { "type": "range", "from": 2023, "to": 2025 }
  },
  "segmentation": ["supportYear"],
  "metrics": ["count"],
  "paging": { "pageSize": 50, "pageNumber": 1 },
  "sort": [ { "field": "supportYear", "direction": "asc" } ]
}
```

Reads as: *"How many **approved** support requests from **associations** in the
**culture** domain were submitted in **2023–2025**, broken down by **year**?"* —
`QuestionTextRenderer` produces the Hebrew form of this in S2.

## JSON Schema

[`query-definition.schema.json`](query-definition.schema.json) is the machine
form of the structural rules above (JSON Schema Draft 2020-12). The example above
validates against it. Registry membership and cross-field checks (reversed range,
unknown field id, tenant existence) are **runtime** validation — FluentValidation
in S2 — and are out of the schema's scope.

To check locally, save the "Worked example" block above to a file and run:

```bash
npx --yes ajv-cli@5 validate --spec=draft2020 \
  -s docs/contracts/query-definition.schema.json \
  -d <that-file>.json
```
