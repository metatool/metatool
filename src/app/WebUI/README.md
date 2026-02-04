# WebUI

This folder contains two parts:

- `frontend` — a Svelte + Tailwind frontend. See `app/WebUI/frontend/README.md` for dev/build steps.
- `Host` — a small WPF WebView2 host that loads the frontend and exposes a hotkey (`Ctrl+Shift+F`). See `app/WebUI/Host/README.md`.

Dev flow:

1. Start the frontend dev server:

```bash
cd app/WebUI/frontend
npm install
npm run dev
```

2. Run the WPF host in dev mode (make sure `DEV_WEBUI` is set):

```powershell
cd app/WebUI/Host
$env:DEV_WEBUI = "1"
dotnet run
```

Production:

1. Build the frontend: `npm run build` in `app/WebUI/frontend` (produces `dist/`).
2. Build the host; the host project copies `frontend/dist` into its output folder automatically.
