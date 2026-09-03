/** Reference lists + filter-field registry from `GET /api/metadata` (`docs/contracts/metadata-model.md`). */

export interface ReferenceItem {
  code: string;
  label: string;
}

/** Named code lists that fill the form's `codeList` controls. Keys match `FilterFieldRegistryEntry.referenceList`. */
export interface References {
  domains: ReferenceItem[];
  bodyTypes: ReferenceItem[];
  statuses: ReferenceItem[];
  districts: ReferenceItem[];
}

export type FieldKind = 'codeList' | 'yearRange';
export type FilterOperator = 'in' | 'range' | 'single';

/** One whitelist entry — drives one form control. */
export interface FilterFieldRegistryEntry {
  id: string;
  label: string;
  kind: FieldKind;
  /** Set for `codeList` only — names the key under `references`. */
  referenceList?: keyof References;
  operators: FilterOperator[];
  segmentable: boolean;
}

export interface MetadataResponse {
  tenantId: string;
  references: References;
  filterFieldRegistry: FilterFieldRegistryEntry[];
}
