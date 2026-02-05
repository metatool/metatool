import { writable } from 'svelte/store';

// Store for hotkeys data
export const hotkeysStore = writable([]);
export const filteredHotkeysStore = writable([]);
export const showSearchStore = writable(false);

/**
 * Send a message to the WebView2 backend
 * @param {Object} data - The message data to send
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
 * Initialize message listeners for WebView2 communication
 * @param {Function} onShowSearch - Callback when showSearch message is received
 * @returns {Function} Cleanup function to remove listeners
 */
export function initMessageListeners(onShowSearch) {
  // WebView2 specific listener
  const webviewMsgHandler = (event) => {
    const data = event.data;
    console.log('Message from backend:', data);

    if (data.type === 'showSearch') {
      console.log('Show search triggered by backend');
      onShowSearch(data);
    }
  };

  // Attach listener
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', webviewMsgHandler);
    console.log('WebView2 message listener initialized');
  } else {
    console.warn('WebView2 API not available - running in non-WebView2 environment');
  }

  // Return cleanup function
  return () => {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.removeEventListener('message', webviewMsgHandler);
    }
  };
}

/**
 * Send close message to backend
 */
export function sendClose() {
  sendMessage({ type: 'close' });
}

/**
 * Send hotkey selection to backend
 * @param {Object} selectedItem - The selected hotkey item
 * @param {number} index - The index of the selected item
 */
export function sendHotkeySelected(selectedItem, index) {
  sendMessage({
    type: 'hotkeySelected',
    index,
    hotkey: selectedItem.hotkey,
    description: selectedItem.description
  });
}
