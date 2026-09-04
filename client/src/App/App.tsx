import { useState, type ReactNode } from 'react';
import { Layout, Menu, Select, Typography } from 'antd';
import {
  BulbOutlined,
  DatabaseOutlined,
  SearchOutlined,
  StarOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { useQueryClient } from '@tanstack/react-query';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { ErrorBoundary } from './ErrorBoundary';
import { NotificationBridge } from './NotificationBridge';
import { routes } from './routes';
import { SEED_USERS, type SeedUser } from '../api/config';
import { getActiveUser, setActiveUser } from '../api/activeUser';
import { SECTION_ICON_COLOR } from '../theme';
import { t } from '../i18n';

const { Header, Content } = Layout;

/** Nav icon per route path — kept here so `routes.tsx` stays a plain data list. */
const NAV_ICONS: Record<string, ReactNode> = {
  '/search': <SearchOutlined aria-hidden style={{ color: SECTION_ICON_COLOR }} />,
  '/saved-queries': <StarOutlined aria-hidden style={{ color: SECTION_ICON_COLOR }} />,
  '/nl-query': <BulbOutlined aria-hidden style={{ color: SECTION_ICON_COLOR }} />,
};

export const App = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [user, setUser] = useState<SeedUser>(getActiveUser);

  const selectUser = (username: string) => {
    if (username === user.username) return;
    const next = setActiveUser(username);
    queryClient.clear();
    setUser(next);
  };

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <NotificationBridge />
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 24,
          paddingInline: 24,
          borderBottom: '1px solid #e7eaf3',
          position: 'sticky',
          top: 0,
          zIndex: 10,
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <span
            style={{
              display: 'grid',
              placeItems: 'center',
              width: 32,
              height: 32,
              borderRadius: 9,
              background: 'linear-gradient(135deg, #3a5bd9, #5b7cff)',
              color: '#fff',
              fontSize: 16,
            }}
          >
            <DatabaseOutlined aria-hidden />
          </span>
          <Typography.Text strong style={{ fontSize: 16, whiteSpace: 'nowrap' }}>
            SupportPlatform
          </Typography.Text>
        </div>
        <nav aria-label={t.navigation.mainNav} style={{ flex: 1, minWidth: 0 }}>
          <Menu
            mode="horizontal"
            selectedKeys={[location.pathname]}
            onClick={({ key }) => navigate(key)}
            style={{ borderBottom: 'none', background: 'transparent' }}
            items={routes.map((route) => ({
              key: route.path,
              label: route.label,
              icon: NAV_ICONS[route.path],
            }))}
          />
        </nav>
        <Select
          aria-label={t.navigation.activeUser}
          value={user.username}
          onChange={selectUser}
          variant="filled"
          prefix={<UserOutlined aria-hidden style={{ color: SECTION_ICON_COLOR }} />}
          options={SEED_USERS.map((u) => ({ value: u.username, label: u.label }))}
          style={{ minWidth: 260 }}
        />
      </Header>
      <Content style={{ padding: '24px 24px 48px' }}>
        <div key={user.username} style={{ maxWidth: 1280, margin: '0 auto' }}>
          <ErrorBoundary>
            <Routes>
              <Route path="/" element={<Navigate to={routes[0].path} replace />} />
              {routes.map((route) => (
                <Route key={route.path} path={route.path} element={route.element} />
              ))}
              <Route path="*" element={<Navigate to={routes[0].path} replace />} />
            </Routes>
          </ErrorBoundary>
        </div>
      </Content>
    </Layout>
  );
};
