import type { ReactNode } from 'react';
import { Space } from 'antd';
import { SECTION_ICON_COLOR } from '../../theme';

interface Props {
  /** A decorative `@ant-design/icons` element — rendered dark-purple and hidden from a11y. */
  icon: ReactNode;
  children: ReactNode;
}

/**
 * Icon + label used for every card title and page heading. Keeps the leading-icon markup and its
 * accent colour in one place.
 */
export function SectionTitle({ icon, children }: Props) {
  return (
    <Space size={8}>
      <span style={{ color: SECTION_ICON_COLOR, display: 'inline-flex' }} aria-hidden>
        {icon}
      </span>
      {children}
    </Space>
  );
}
