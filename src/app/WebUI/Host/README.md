# WebView2 Host

Run the WPF host (loads frontend):

```powershell
cd app/WebUI/Host
dotnet build
dotnet run
```

By default the host looks for a dev server at `http://localhost:5173` when `DEV_WEBUI` environment variable is set.

Hotkey: `Ctrl+Shift+F` will bring up the window and post a `showSearch` message to the web UI.

When bundling for production, place the frontend build under `app/WebUI/frontend/dist` so the host can load `index.html`.
