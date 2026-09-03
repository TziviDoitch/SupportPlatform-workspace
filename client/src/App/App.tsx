import { Layout, Menu, Typography } from 'antd';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { ErrorBoundary } from './ErrorBoundary';
import { NotificationBridge } from './NotificationBridge';
import { routes } from './routes';

const { Header, Content } = Layout;

export function App() {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <NotificationBridge />
      <Header style={{ display: 'flex', alignItems: 'center', gap: 24 }}>
        <Typography.Text strong style={{ color: '#fff' }}>
          SupportPlatform
        </Typography.Text>
        <nav aria-label="ניווט ראשי" style={{ flex: 1, minWidth: 0 }}>
          <Menu
            theme="dark"
            mode="horizontal"
            selectedKeys={[location.pathname]}
            onClick={({ key }) => navigate(key)}
            items={routes.map((route) => ({ key: route.path, label: route.label }))}
          />
        </nav>
      </Header>
      <Content style={{ padding: 24 }}>
        <ErrorBoundary>
          <Routes>
            <Route path="/" element={<Navigate to={routes[0].path} replace />} />
            {routes.map((route) => (
              <Route key={route.path} path={route.path} element={route.element} />
            ))}
            <Route path="*" element={<Navigate to={routes[0].path} replace />} />
          </Routes>
        </ErrorBoundary>
      </Content>
    </Layout>
  );
}
