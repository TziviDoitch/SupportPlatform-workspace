import { Col, Form, Row, Select } from 'antd';
import { CalendarOutlined } from '@ant-design/icons';
import type { FilterFieldRegistryEntry } from '../../../models/metadata';
import { MIN_YEAR, type YearInput } from '../buildQueryDefinition';
import { t } from '../../../i18n';

interface Props {
  entry: FilterFieldRegistryEntry;
  value: YearInput;
  onChange: (year: YearInput | undefined) => void;
}

const MAX_YEAR = new Date().getFullYear() + 1;

const YEAR_OPTIONS = Array.from({ length: MAX_YEAR - MIN_YEAR + 1 }, (_, i) => {
  const year = MAX_YEAR - i;
  return { value: year, label: String(year) };
});

export const YearRangeField = ({ entry, value, onChange }: Props) => {
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
            aria-label={`${entry.label} — ${t.yearRange.fromLabel}`}
            placeholder={t.yearRange.fromPlaceholder}
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
            aria-label={`${entry.label} — ${t.yearRange.toLabel}`}
            placeholder={t.yearRange.toPlaceholder}
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
};
