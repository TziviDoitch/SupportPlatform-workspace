import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

// Unmount React trees between tests (no Vitest globals, so RTL can't auto-register this).
afterEach(() => {
  cleanup();
});

// jsdom lacks APIs that antd's layout components probe. Stub the minimum.

class ResizeObserverMock {
  observe(): void {}
  unobserve(): void {}
  disconnect(): void {}
}

globalThis.ResizeObserver ??= ResizeObserverMock as unknown as typeof ResizeObserver;

if (!window.matchMedia) {
  window.matchMedia = ((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  })) as unknown as typeof window.matchMedia;
}
