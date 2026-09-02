# Contract — Metadata & Filter Field Registry

> Frozen in S0-d. Source: `IMPLEMENTATION_PLAN.md` §5, §6 S1, §8 Q1.

`GET /api/metadata` is the one call the client makes before rendering the search
form. It returns everything needed to build the form **and** the whitelist the
server uses to validate a `QueryDefinition`. Adding a support domain, body type,
district, or a whole new filter field is a **data** change (reference rows +
registry row + seed) — no code change (§8 Q1).

## `GET /api/metadata?tenantId=<id>` → `200`

```json
{
  "tenantId": "culture-sport-admin",
  "references": {
    "domains":   [ { "code": "culture", "label": "תרבות" },
                   { "code": "sport",   "label": "ספורט" } ],
    "bodyTypes": [ { "code": "association", "label": "עמותה" },
                   { "code": "company",     "label": "חברה" } ],
    "statuses":  [ { "code": "approved", "label": "מאושר" },
                   { "code": "pending",  "label": "בבדיקה" },
                   { "code": "rejected", "label": "נדחה" } ],
    "districts": [ { "code": "north",  "label": "צפון" },
                   { "code": "center", "label": "מרכז" },
                   { "code": "south",  "label": "דרום" } ]
  },
  "filterFieldRegistry": [
    { "id": "bodyType",      "label": "סוג גוף",   "kind": "codeList", "referenceList": "bodyTypes", "operators": ["in"],    "segmentable": true  },
    { "id": "supportDomain", "label": "תחום תמיכה", "kind": "codeList", "referenceList": "domains",   "operators": ["in"],    "segmentable": true  },
    { "id": "status",        "label": "סטטוס",     "kind": "codeList", "referenceList": "statuses",  "operators": ["in"],    "segmentable": false },
    { "id": "district",      "label": "מחוז",      "kind": "codeList", "referenceList": "districts", "operators": ["in"],    "segmentable": true  },
    { "id": "supportYear",   "label": "שנת תמיכה", "kind": "yearRange", "operators": ["range", "single"], "segmentable": true }
  ]
}
```

## `references`

An object of named code lists. Each entry is `{ code, label }` — `code` is the
stable identifier stored on rows and used in `QueryDefinition.filters`; `label` is
the Hebrew display text. Backing tables (§5): `reference_domains`,
`reference_body_types`, `reference_statuses`, `reference_districts`.

Reference lists are global for the PoC; the response still carries them per call
so the client has a single source.

## `filterFieldRegistry`

An array of registry entries — the **whitelist**. Backing table:
`filter_field_registry`.

| Property | Type | Meaning |
|---|---|---|
| `id` | string | Canonical field id. The only spelling used in `QueryDefinition` (`filters` keys, `segmentation`, `sort[].field`). |
| `label` | string | Hebrew label for the form control. |
| `kind` | `codeList` \| `yearRange` | Drives the control type and the accepted filter value shape. |
| `referenceList` | string | For `codeList` only — names the key under `references` that fills the control's options. |
| `operators` | string[] | Subset of `in`, `range`, `single`. What `DynamicQueryBuilder` may emit for this field (`in` for code lists; `range` / `single` for `yearRange`). |
| `segmentable` | boolean | Whether the id may appear in `QueryDefinition.segmentation`. |

## How the client uses it

- One form control per registry entry, in array order.
  - `kind: "codeList"` → multi-select filled from `references[referenceList]`,
    emits `filters[id] = string[]`.
  - `kind: "yearRange"` → from/to (or single) picker, emits
    `filters[id] = { type: "range", from, to }` or `{ type: "single", value }`.
- Segmentation picker offers only entries with `segmentable: true`.
- Nothing about the form is hard-coded — a new registry row produces a new
  control on next load.

## How the server uses it

`DynamicQueryBuilder` loads the registry for the tenant and:

1. Rejects (400) any `filters` key, `segmentation` entry, or `sort[].field` whose
   id is not in the registry.
2. Rejects (400) a filter value whose shape doesn't match the entry's `kind`, or
   an operator not in `operators`.
3. Builds the `IQueryable` only from validated, whitelisted fields.

This is the §3.4 red line — the dynamic query is never assembled from raw client
field names. See [`query-definition.md`](query-definition.md) and
[`error-model.md`](error-model.md).
