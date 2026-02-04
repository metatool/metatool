# WebUI Frontend

Dev:

```bash
cd app/WebUI/frontend
npm install
npm run dev
```

Build (for production):

```bash
npm run build
```

Behavior:
- The frontend listens for `message` events with `{ type: 'showSearch' }` to show the search bar.
- When running inside WebView2, host messages are handled via `window.chrome.webview`.
