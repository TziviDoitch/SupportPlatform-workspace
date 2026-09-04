import { useState } from 'react';
import { Input, Modal } from 'antd';
import type { SavedQuery } from '../../../models/savedQuery';
import { t } from '../../../i18n';

interface Props {
  query: SavedQuery | null;
  confirmLoading?: boolean;
  onCancel: () => void;
  onSubmit: (name: string) => void;
}

export const RenameQueryModal = ({ query, confirmLoading, onCancel, onSubmit }: Props) => {
  const [name, setName] = useState(query?.name ?? '');
  const trimmed = name.trim();

  return (
    <Modal
      open={query !== null}
      title={t.savedQueries.renameTitle}
      okText={t.common.save}
      cancelText={t.common.cancel}
      okButtonProps={{ disabled: trimmed.length === 0 }}
      confirmLoading={confirmLoading}
      onCancel={onCancel}
      onOk={() => onSubmit(trimmed)}
    >
      <Input
        aria-label={t.savedQueries.renameLabel}
        value={name}
        onChange={(e) => setName(e.target.value)}
        onPressEnter={() => trimmed.length > 0 && onSubmit(trimmed)}
      />
    </Modal>
  );
};
