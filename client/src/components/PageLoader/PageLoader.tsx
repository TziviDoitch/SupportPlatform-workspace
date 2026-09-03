import { Spin } from 'antd';

/** Centered page-level loading state — the bare `<Spin/>` renders top-left and looks unfinished. */
export function PageLoader() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
      <Spin />
    </div>
  );
}
