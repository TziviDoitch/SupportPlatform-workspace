export interface ReferenceItem {
  code: string;
  label: string;
}

export interface References {
  domains: ReferenceItem[];
  bodyTypes: ReferenceItem[];
  statuses: ReferenceItem[];
  districts: ReferenceItem[];
}

export type FieldKind = 'codeList' | 'yearRange';
export type FilterOperator = 'in' | 'range' | 'single';

export interface FilterFieldRegistryEntry {
  id: string;
  label: string;
  kind: FieldKind;
  referenceList?: keyof References;
  operators: FilterOperator[];
  segmentable: boolean;
}

export interface MetadataResponse {
  tenantId: string;
  references: References;
  filterFieldRegistry: FilterFieldRegistryEntry[];
}
