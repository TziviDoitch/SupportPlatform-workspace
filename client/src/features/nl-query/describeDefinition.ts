import { labelForCode, labelForField } from '../../lib/labels';
import type { MetadataResponse } from '../../models/metadata';
import type { FilterValue, QueryDefinition } from '../../models/queryDefinition';

export interface InterpretedField {
  label: string;
  value: string;
}

const SEGMENTATION_LABEL = 'קיבוץ לפי';

/**
 * The definition as a label/value list, for the review panel — the same registry and reference
 * labels the search form is built from. It reads the definition, it does not compose Hebrew
 * prose: the sentence is `interpretationText` from the server.
 */
export function describeDefinition(
  definition: QueryDefinition,
  metadata: MetadataResponse,
): InterpretedField[] {
  const fields: InterpretedField[] = [];

  for (const entry of metadata.filterFieldRegistry) {
    const value = definition.filters[entry.id];
    if (value === undefined) continue;

    // The registry is server data: a row may name a list this client does not carry.
    const options = (entry.referenceList && metadata.references[entry.referenceList]) || [];
    fields.push({
      label: entry.label,
      value: valueText(value, options),
    });
  }

  if (definition.segmentation.length > 0) {
    const labels = definition.segmentation.map((id) =>
      labelForField(metadata.filterFieldRegistry, id),
    );
    fields.push({ label: SEGMENTATION_LABEL, value: labels.join(', ') });
  }

  return fields;
}

function valueText(value: FilterValue, options: { code: string; label: string }[]): string {
  if (Array.isArray(value)) {
    return value.map((code) => labelForCode(options, code)).join(' או ');
  }
  return value.type === 'range' ? `${value.from}–${value.to}` : String(value.value);
}
