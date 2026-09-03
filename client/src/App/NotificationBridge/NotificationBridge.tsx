import { useEffect } from 'react';
import { App } from 'antd';
import { setNotificationInstance } from '../../api/notificationHost';

/**
 * Bridges antd's context-aware `notification` API (theme + RTL direction) to the non-React `http`
 * seam via {@link setNotificationInstance}. Must render inside antd's `<App>`. Renders nothing.
 */
export function NotificationBridge() {
  const { notification } = App.useApp();
  useEffect(() => {
    setNotificationInstance(notification);
    return () => setNotificationInstance(null);
  }, [notification]);
  return null;
}
