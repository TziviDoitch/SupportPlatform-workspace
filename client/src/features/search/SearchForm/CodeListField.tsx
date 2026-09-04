import { Form, Select } from 'antd';
import type { FilterFieldRegistryEntry, References } from '../../../models/metadata';

interface Props {
  entry: FilterFieldRegistryEntry;
  references: References;
  value: string[];
  onChange: (codes: string[]) => void;
}

export const CodeListField = ({ entry, references, value, onChange }: Props) => {
  const items = entry.referenceList ? references[entry.referenceList] : [];
  return (
    <Form.Item label={entry.label}>
      <Select
        mode="multiple"
        allowClear
        placeholder={`כל ה${entry.label}`}
        value={value}
        onChange={onChange}
        options={items.map((i) => ({ value: i.code, label: i.label }))}
        style={{ width: '100%' }}
      />
    </Form.Item>
  );
};
