import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button, Result } from 'antd';
import { t } from '../../i18n';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

/**
 * Last-resort boundary for render-time exceptions (a bad server shape, a chart edge case).
 * Data/HTTP errors are surfaced earlier by `api/http.ts`; this catches what slips past and
 * keeps a thrown render from blanking the whole app.
 */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error('Unhandled render error', error, info.componentStack);
  }

  render(): ReactNode {
    if (!this.state.hasError) return this.props.children;

    return (
      <Result
        status="error"
        title={t.error.title}
        subTitle={t.error.message}
        extra={
          <Button type="primary" onClick={() => window.location.reload()}>
            {t.common.reload}
          </Button>
        }
      />
    );
  }
}
