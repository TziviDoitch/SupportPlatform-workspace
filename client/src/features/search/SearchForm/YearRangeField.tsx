import { Form, InputNumber, Space } from 'antd';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { YearInput } from '../buildQueryDefinition';

interface Props {
  entry: FilterFieldRegistryEntry;
  value: YearInput;
  onChange: (year: YearInput | undefined) => void;
}

/** From/to year pickers for a `yearRange` registry field. Empty on both ends clears the filter. */
export function YearRangeField({ entry, value, onChange }: Props) {
  const update = (patch: Partial<YearInput>) => {
    const next: YearInput = { ...value, ...patch };
    onChange(next.from == null && next.to == null ? undefined : next);
  };
  return (
    <Form.Item label={entry.label}>
      <Space>
        <InputNumber
          placeholder="משנה"
          value={value.from ?? null}
          onChange={(v) => update({ from: v == null ? undefined : Number(v) })}
        />
        <InputNumber
          placeholder="עד שנה"
          value={value.to ?? null}
          onChange={(v) => update({ to: v == null ? undefined : Number(v) })}
        />
      </Space>
    </Form.Item>
  );
}
