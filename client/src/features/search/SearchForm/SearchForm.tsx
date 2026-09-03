import { Card, Form, Select } from 'antd';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { FieldValue, SearchFormState, YearInput } from '../buildQueryDefinition';
import { CodeListField } from './CodeListField';
import { YearRangeField } from './YearRangeField';

interface Props {
  registry: FilterFieldRegistryEntry[];
  references: References;
  state: SearchFormState;
  onFieldChange: (fieldId: string, value: FieldValue | undefined) => void;
  onSegmentationChange: (ids: string[]) => void;
}

/** Dynamic search form: one control per registry entry, in registry order. Nothing is hard-coded. */
export function SearchForm({
  registry,
  references,
  state,
  onFieldChange,
  onSegmentationChange,
}: Props) {
  const segmentable = registry.filter((e) => e.segmentable);

  return (
    <Card size="small" title="סינון" style={{ marginBottom: 16 }}>
      <Form layout="vertical">
        {registry.map((entry) =>
          entry.kind === 'codeList' ? (
            <CodeListField
              key={entry.id}
              entry={entry}
              references={references}
              value={asCodes(state.values[entry.id])}
              onChange={(codes) => onFieldChange(entry.id, codes.length ? codes : undefined)}
            />
          ) : (
            <YearRangeField
              key={entry.id}
              entry={entry}
              value={asYear(state.values[entry.id])}
              onChange={(year) => onFieldChange(entry.id, year)}
            />
          ),
        )}

        <Form.Item label="פילוח">
          <Select
            mode="multiple"
            allowClear
            placeholder="ללא פילוח"
            value={state.segmentation}
            onChange={onSegmentationChange}
            options={segmentable.map((e) => ({ value: e.id, label: e.label }))}
          />
        </Form.Item>
      </Form>
    </Card>
  );
}

const asCodes = (v: FieldValue | undefined): string[] => (Array.isArray(v) ? v : []);
const asYear = (v: FieldValue | undefined): YearInput => (v && !Array.isArray(v) ? v : {});
