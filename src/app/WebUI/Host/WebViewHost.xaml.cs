using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Windows.Input;
using System.Diagnostics;

namespace Metatool.WebViewHost
{
    public partial class WebViewHost : Window
    {
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        public WebViewHost()
        {
            InitializeComponent();
            Loaded += WebViewHost_Loaded;
            Closed += WebViewHost_Closed;
        }

        private async void WebViewHost_Loaded(object sender, RoutedEventArgs e)
        {
            var dev = Environment.GetEnvironmentVariable("DEV_WEBUI");

            if (!string.IsNullOrEmpty(dev))
            {
                // Set environment variable for remote debugging BEFORE creating WebView2
                Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=9222");

                await webView.EnsureCoreWebView2Async();

                Debug.WriteLine("Remote debugging enabled on port 9222. Open edge://inspect to debug.");

                webView.Source = new Uri("http://localhost:5173");
            }
            else
            {
                await webView.EnsureCoreWebView2Async();
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var index = System.IO.Path.Combine(exeDir, "frontend", "dist", "index.html");
                if (System.IO.File.Exists(index))
                    webView.Source = new Uri(index);
            }

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            var hwnd = new WindowInteropHelper(this).Handle;
            var vk = (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.F);
            var registered = RegisterHotKey(hwnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, vk);
            Debug.WriteLine($"RegisterHotKey(hwnd={hwnd}, id={HOTKEY_ID}, mods=0x{MOD_CONTROL | MOD_SHIFT:X}, vk={vk}) = {registered}");
            if (!registered)
            {
                var err = Marshal.GetLastWin32Error();
                Debug.WriteLine($"RegisterHotKey failed with error code: {err}");
            }
            var source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
        }

        private void WebViewHost_Closed(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hwnd, HOTKEY_ID);
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.WebMessageAsJson;
            System.Diagnostics.Debug.WriteLine("Message from web: " + json);
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("type", out var t))
                {
                    var type = t.GetString();
                    if (type == "close")
                    {
                        Dispatcher.Invoke(Close);
                    }
                    else if (type == "searchPerformed")
                    {
                        var query = doc.RootElement.GetProperty("query").GetString();

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse web message: " + ex.Message);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                Debug.WriteLine($"WM_HOTKEY received: hotkeyId={hotkeyId}, HOTKEY_ID={HOTKEY_ID}");
                if (hotkeyId == HOTKEY_ID)
                {
                    Debug.WriteLine("Calling ShowSearch()");
                    ShowSearch();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private async void ShowSearch()
        {
            Debug.WriteLine("ShowSearch() called");
            Dispatcher.Invoke(async () => {
                if (!IsVisible)
                {
                    Debug.WriteLine("Window not visible, calling Show()");
                    // Set initial size
                    this.Width = 700;
                    this.Height = 300;
                    Show();
                }
                Debug.WriteLine("Calling Activate()");
                Activate();
                await webView.EnsureCoreWebView2Async();
                Debug.WriteLine("Executing postMessage script via WebView2 postMessage");

                // Create the message object with type and hotkeys data
                var hotkeyJson = MockHotkeys.GetJson();
                var messageJson = $"{{\"type\":\"showSearch\",\"hotkeys\":{hotkeyJson}}}";

                webView.CoreWebView2.PostWebMessageAsJson(messageJson);

                // Schedule a resize after content is rendered
                await Task.Delay(100);
                ResizeToContent();
            });
        }

        private void ResizeToContent()
        {
            // Use ExecuteScriptAsync to get the document body scrollHeight
            _ = webView.CoreWebView2.ExecuteScriptAsync(
                "document.documentElement.scrollHeight.toString()"
            ).ContinueWith(async task =>
            {
                try
                {
                    var result = task.Result;
                    if (int.TryParse(result.Trim('"'), out int contentHeight))
                    {
                        Debug.WriteLine($"Content height: {contentHeight}");
                        Dispatcher.Invoke(() =>
                        {
                            // Add padding for border and margins
                            var newHeight = Math.Min(contentHeight + 40, 900);

                            // Set WebView2 control height explicitly
                            webView.Height = newHeight;

                            // Set Window height
                            this.Height = newHeight;

                            Debug.WriteLine($"Window and WebView2 height adjusted to: {newHeight}");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error measuring content: {ex.Message}");
                }
            });
        }

    }
}
