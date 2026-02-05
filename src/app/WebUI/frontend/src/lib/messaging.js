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
  const webviewHandler = (event) => {
    const data = event.data;
    console.log('Message from backend:', data);

    if (data.type === 'showSearch') {
      console.log('Show search triggered by backend');
      onShowSearch(data);
    }
  };

  // Window message listener for broader compatibility
  const windowHandler = (event) => {
    if (event.data && event.data.type === 'showSearch') {
      console.log('Show search triggered via window message');
      onShowSearch(event.data);
      document.dispatchEvent(new CustomEvent('focus-search'));
    }
  };

  // Attach listeners
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', webviewHandler);
    console.log('WebView2 message listener initialized');
  } else {
    console.warn('WebView2 API not available - running in non-WebView2 environment');
  }

  window.addEventListener('message', windowHandler);

  // Return cleanup function
  return () => {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.removeEventListener('message', webviewHandler);
    }
    window.removeEventListener('message', windowHandler);
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
