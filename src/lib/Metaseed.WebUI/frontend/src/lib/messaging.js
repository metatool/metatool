import { writable } from 'svelte/store';

export const hotkeysStore = writable([]);
export const filteredHotkeysStore = writable([]);
export const showSearchStore = writable(false);

/**
 * @param {Object} data
 */
export function sendMessage(data) {
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(data);
    console.log('Message sent to backend:', data);
  } else {
    console.warn('WebView2 API not available');
  }
}

/**
 * @param {Object} handlers - { onShowSearch, onShowLogs, onAddLog }
 * @returns {Function} Cleanup function
 */
export function initMessageListeners(handlers) {
  const webviewMsgHandler = (event) => {
    const data = event.data;

    if (data.type === 'showSearch') {
      handlers.onShowSearch?.(data);
    } else if (data.type === 'showLogs') {
      handlers.onShowLogs?.(data);
    } else if (data.type === 'addLog') {
      handlers.onAddLog?.(data);
    }
  };

  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', webviewMsgHandler);
    console.log('WebView2 message listener initialized');
  } else {
    console.warn('WebView2 API not available - running in non-WebView2 environment');
  }

  return () => {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.removeEventListener('message', webviewMsgHandler);
    }
  };
}

export function sendClose() {
  sendMessage({ type: 'close' });
}

/**
 * @param {Object} selectedItem
 * @param {number} index
 */
export function sendHotkeySelected(selectedItem, index) {
  sendMessage({
    type: 'hotkeySelected',
    index,
    hotkey: selectedItem.hotkey,
    description: selectedItem.description
  });
}
