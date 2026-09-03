import { useState } from 'react';
import { Button, Input, Modal } from 'antd';
import { notifySuccess } from '../../../api/notificationHost';
import type { QueryDefinition } from '../../../models/queryDefinition';
import { useCreateSavedQuery } from '../hooks/useSavedQueries';

interface Props {
  definition: QueryDefinition;
}

/** Saves the current search definition as a named saved query. Lives on the search screen. */
export function SaveQueryButton({ definition }: Props) {
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
          notifySuccess({ message: 'השאילתה נשמרה', description: trimmed });
          setOpen(false);
          setName('');
        },
      },
    );
  };

  return (
    <>
      <Button onClick={() => setOpen(true)}>שמור שאילתה</Button>
      <Modal
        open={open}
        title="שמירת שאילתה"
        okText="שמור"
        cancelText="ביטול"
        okButtonProps={{ disabled: trimmed.length === 0 }}
        confirmLoading={create.isPending}
        onCancel={() => setOpen(false)}
        onOk={submit}
      >
        <Input
          aria-label="שם שאילתה"
          placeholder="לדוגמה: עמותות תרבות מאושרות"
          value={name}
          onChange={(e) => setName(e.target.value)}
          onPressEnter={submit}
        />
      </Modal>
    </>
  );
}
