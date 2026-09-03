import { Col, Form, Row, Select } from 'antd';
import { CalendarOutlined } from '@ant-design/icons';
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
const MAX_YEAR = new Date().getFullYear() + 1;

/** Newest year first — the common case for support data is a recent year. */
const YEAR_OPTIONS = Array.from({ length: MAX_YEAR - MIN_YEAR + 1 }, (_, i) => {
  const year = MAX_YEAR - i;
  return { value: year, label: String(year) };
});

/**
 * From/to year pickers for a `yearRange` registry field — two bounded dropdowns (no free-form
 * numbers, §7). Empty on both ends clears the filter; one end set becomes a single-year filter
 * downstream. The "to" list hides years before the chosen "from".
 */
export function YearRangeField({ entry, value, onChange }: Props) {
  const update = (patch: Partial<YearInput>) => {
    const next: YearInput = { ...value, ...patch };
    onChange(next.from == null && next.to == null ? undefined : next);
  };

  const toOptions =
    value.from == null ? YEAR_OPTIONS : YEAR_OPTIONS.filter((o) => o.value >= value.from!);

  return (
    <Form.Item label={entry.label}>
      <Row gutter={8}>
        <Col span={12}>
          <Select
            aria-label={`${entry.label} — משנה`}
            placeholder="משנה"
            allowClear
            showSearch
            suffixIcon={<CalendarOutlined aria-hidden />}
            options={YEAR_OPTIONS}
            value={value.from ?? undefined}
            onChange={(v) => update({ from: v == null ? undefined : Number(v) })}
            style={{ width: '100%' }}
          />
        </Col>
        <Col span={12}>
          <Select
            aria-label={`${entry.label} — עד שנה`}
            placeholder="עד שנה"
            allowClear
            showSearch
            suffixIcon={<CalendarOutlined aria-hidden />}
            options={toOptions}
            value={value.to ?? undefined}
            onChange={(v) => update({ to: v == null ? undefined : Number(v) })}
            style={{ width: '100%' }}
          />
        </Col>
      </Row>
    </Form.Item>
  );
}
