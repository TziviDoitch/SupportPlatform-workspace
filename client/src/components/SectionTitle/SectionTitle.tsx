import type { ReactNode } from 'react';
import { Space } from 'antd';
import { SECTION_ICON_COLOR } from '../../theme';

interface Props {
  icon: ReactNode;
  children: ReactNode;
}

export const SectionTitle = ({ icon, children }: Props) => {
  return (
    <Space size={8}>
      <span style={{ color: SECTION_ICON_COLOR, display: 'inline-flex' }} aria-hidden>
        {icon}
      </span>
      {children}
    </Space>
  );
};
