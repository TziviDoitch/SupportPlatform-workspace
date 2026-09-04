import { useState } from 'react';
import { Button, Card, Col, Divider, Form, Row, Select, Space } from 'antd';
import {
  DownOutlined,
  FilterOutlined,
  ReloadOutlined,
  SearchOutlined,
  UpOutlined,
} from '@ant-design/icons';
import { SectionTitle } from '../../../components/SectionTitle';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';
import type { FieldValue, SearchFormState, YearInput } from '../buildQueryDefinition';
import { CodeListField } from './CodeListField';
import { YearRangeField } from './YearRangeField';

interface Props {
  registry: FilterFieldRegistryEntry[];
  references: References;
  state: SearchFormState;
  isSearching?: boolean;
  onFieldChange: (fieldId: string, value: FieldValue | undefined) => void;
  onGraphFieldsChange: (ids: string[]) => void;
  onSearch: () => void;
  onClear: () => void;
}

/** A wide field (year range) takes more grid columns than a single-control field. */
const colSpan = (kind: FilterFieldRegistryEntry['kind']) =>
  kind === 'yearRange'
    ? { xs: 24, md: 12, xl: 8 }
    : { xs: 24, md: 12, xl: 6 };

/** Dynamic search form: one control per registry entry, in registry order. Nothing is hard-coded. */
export function SearchForm({
  registry,
  references,
  state,
  isSearching,
  onFieldChange,
  onGraphFieldsChange,
  onSearch,
  onClear,
}: Props) {
  const [open, setOpen] = useState(true);
  const segmentable = registry.filter((e) => e.segmentable);

  return (
    <Card
      title={<SectionTitle icon={<FilterOutlined />}>מאפייני חיפוש</SectionTitle>}
      extra={
        <Button type="text" onClick={() => setOpen((v) => !v)}>
          {open ? 'הסתרת מאפייני חיפוש' : 'הצגת מאפייני חיפוש'}
          {open ? <UpOutlined aria-hidden /> : <DownOutlined aria-hidden />}
        </Button>
      }
      style={{ marginBottom: 20 }}
      styles={{ body: open ? undefined : { display: 'none' } }}
    >
      <Form layout="vertical">
        <Row gutter={[20, 0]}>
          {registry.map((entry) => (
            <Col key={entry.id} {...colSpan(entry.kind)}>
              {entry.kind === 'codeList' ? (
                <CodeListField
                  entry={entry}
                  references={references}
                  value={asCodes(state.values[entry.id])}
                  onChange={(codes) => onFieldChange(entry.id, codes.length ? codes : undefined)}
                />
              ) : (
                <YearRangeField
                  entry={entry}
                  value={asYear(state.values[entry.id])}
                  onChange={(year) => onFieldChange(entry.id, year)}
                />
              )}
            </Col>
          ))}

          <Col {...colSpan('codeList')}>
            <Form.Item
              label="הוספת גרף לפי"
              tooltip="גרף לכל שדה שנבחר. הטבלה תמיד מציגה תחום תמיכה, מחוז ושנת תמיכה."
            >
              <Select
                mode="multiple"
                allowClear
                placeholder="בחרו שדה (למשל: מחוז)"
                value={state.graphFields}
                onChange={onGraphFieldsChange}
                options={segmentable.map((e) => ({ value: e.id, label: e.label }))}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
        </Row>

        <Divider style={{ margin: '4px 0 16px' }} />

        <Space>
          <Button type="primary" icon={<SearchOutlined aria-hidden />} loading={isSearching} onClick={onSearch}>
            חיפוש
          </Button>
          <Button type="link" icon={<ReloadOutlined aria-hidden />} onClick={onClear}>
            ניקוי מאפייני חיפוש
          </Button>
        </Space>
      </Form>
    </Card>
  );
}

const asCodes = (v: FieldValue | undefined): string[] => (Array.isArray(v) ? v : []);
const asYear = (v: FieldValue | undefined): YearInput => (v && !Array.isArray(v) ? v : {});
