import { useState } from 'react';
import { Button, Input, Modal } from 'antd';
import { notifySuccess } from '../../../api/notificationHost';
import type { QueryDefinition } from '../../../models/queryDefinition';
import { useCreateSavedQuery } from '../hooks/useSavedQueries';
import { t } from '../../../i18n';

interface Props {
  definition: QueryDefinition;
}

export const SaveQueryButton = ({ definition }: Props) => {
  const create = useCreateSavedQuery();
  const [open, setOpen] = useState(false);
  const [name, setName] = useState('');
  const trimmed = name.trim();

  const submit = () => {
    if (trimmed.length === 0) return;
    create.mutate(
      { name: trimmed, definition },
      {
        onSuccess: () => {
          notifySuccess({ message: t.savedQueries.saveNotification, description: trimmed });
          setOpen(false);
          setName('');
        },
      },
    );
  };

  return (
    <>
      <Button onClick={() => setOpen(true)}>{t.savedQueries.saveButton}</Button>
      <Modal
        open={open}
        title={t.savedQueries.saveTitle}
        okText={t.common.save}
        cancelText={t.common.cancel}
        okButtonProps={{ disabled: trimmed.length === 0 }}
        confirmLoading={create.isPending}
        onCancel={() => setOpen(false)}
        onOk={submit}
      >
        <Input
          aria-label={t.savedQueries.saveLabel}
          placeholder={t.savedQueries.savePlaceholder}
          value={name}
          onChange={(e) => setName(e.target.value)}
          onPressEnter={submit}
        />
      </Modal>
    </>
  );
};
