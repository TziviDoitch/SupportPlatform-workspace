import { Spin } from 'antd';

interface Props {
  /** Pass `"large"` for the results area; omit for the default page-level size. */
  size?: 'large';
}

/** Centered rotating spinner — the bare `<Spin/>` renders top-left and looks unfinished. */
export function PageLoader({ size }: Props) {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
      <Spin size={size} />
    </div>
  );
}
