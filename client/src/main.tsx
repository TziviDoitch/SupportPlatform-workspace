import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { App as AntdApp, ConfigProvider } from 'antd';
import heIL from 'antd/locale/he_IL';
import { queryClient } from './state/queryClient';
import { theme } from './theme';
import { App } from './App';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider direction="rtl" locale={heIL} theme={theme} card={{ variant: 'borderless' }}>
        <AntdApp>
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </AntdApp>
      </ConfigProvider>
    </QueryClientProvider>
  </StrictMode>,
);
