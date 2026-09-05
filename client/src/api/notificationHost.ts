import { notification as staticNotification } from 'antd';

export interface Notice {
  message: string;
  description?: string;
}

type Sink = {
  error: (notice: Notice) => void;
  success: (notice: Notice) => void;
};

// Set from inside the antd <App> tree (see App/NotificationBridge) so toasts inherit theme + RTL
// direction. Null until then — and in non-UI contexts like unit tests.
let instance: Sink | null = null;

export function setNotificationInstance(next: Sink | null): void {
  instance = next;
}

// Uses the context-aware <App> instance when one is registered, otherwise antd's static API.
export function notifyError(notice: Notice): void {
  (instance ?? staticNotification).error(notice);
}

export function notifySuccess(notice: Notice): void {
  (instance ?? staticNotification).success(notice);
}
