import { useState } from 'react';
import { Input, Modal } from 'antd';
import type { SavedQuery } from '../../../models/savedQuery';

interface Props {
  query: SavedQuery | null;
  confirmLoading?: boolean;
  onCancel: () => void;
  onSubmit: (name: string) => void;
}

/**
 * Rename an existing saved query. Open when `query` is set. The parent keys this by query id so
 * each open remounts with the right initial name.
 */
export function RenameQueryModal({ query, confirmLoading, onCancel, onSubmit }: Props) {
  const [name, setName] = useState(query?.name ?? '');
  const trimmed = name.trim();

  return (
    <Modal
      open={query !== null}
      title="שינוי שם שאילתה"
      okText="שמור"
      cancelText="ביטול"
      okButtonProps={{ disabled: trimmed.length === 0 }}
      confirmLoading={confirmLoading}
      onCancel={onCancel}
      onOk={() => onSubmit(trimmed)}
    >
      <Input
        aria-label="שם שאילתה"
        value={name}
        onChange={(e) => setName(e.target.value)}
        onPressEnter={() => trimmed.length > 0 && onSubmit(trimmed)}
      />
    </Modal>
  );
}
