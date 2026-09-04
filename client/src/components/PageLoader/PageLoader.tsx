import { Spin } from 'antd';

interface Props {
  size?: 'large';
}

export const PageLoader = ({ size }: Props) => {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
      <Spin size={size} />
    </div>
  );
};
