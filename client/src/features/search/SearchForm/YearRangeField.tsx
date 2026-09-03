import { Form, InputNumber, Space } from 'antd';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import type { YearInput } from '../buildQueryDefinition';

interface Props {
  entry: FilterFieldRegistryEntry;
  value: YearInput;
  onChange: (year: YearInput | undefined) => void;
}

/** Plausible calendar-year bounds for the support-year inputs. The server is still the authority
 *  (`docs/contracts/query-definition.md`); these just keep the control from emitting nonsense. */
const MIN_YEAR = 2000;
const MAX_YEAR = 2100;

/**
 * From/to year pickers for a `yearRange` registry field — bounded integers (no free-form numbers,
 * §7). Empty on both ends clears the filter; one end set becomes a single-year filter downstream.
 */
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
          min={MIN_YEAR}
          max={MAX_YEAR}
          step={1}
          precision={0}
          value={value.from ?? null}
          onChange={(v) => update({ from: v == null ? undefined : Number(v) })}
        />
        <InputNumber
          placeholder="עד שנה"
          min={MIN_YEAR}
          max={MAX_YEAR}
          step={1}
          precision={0}
          value={value.to ?? null}
          onChange={(v) => update({ to: v == null ? undefined : Number(v) })}
        />
      </Space>
    </Form.Item>
  );
}
