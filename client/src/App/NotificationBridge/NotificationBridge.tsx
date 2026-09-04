import { useEffect } from 'react';
import { App } from 'antd';
import { setNotificationInstance } from '../../api/notificationHost';

export const NotificationBridge = () => {
  const { notification } = App.useApp();
  useEffect(() => {
    setNotificationInstance(notification);
    return () => setNotificationInstance(null);
  }, [notification]);
  return null;
};
