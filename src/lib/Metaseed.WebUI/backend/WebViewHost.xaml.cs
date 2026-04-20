using System.ComponentModel;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Diagnostics;
using System.IO;

namespace Metatool.WebViewHost
{
    public record TipItem(string hotkey, string description);
    public record LogEntryDto(string timestamp, string level, string category, string message);

    public partial class WebViewHost : Window
    {
        private readonly string dev;
        private CoreWebView2Environment _webViewEnv;
        private bool _logViewVisible;

        public WebViewHost()
        {
            InitializeComponent();
            // defined in launchSettings.json
            dev = Environment.GetEnvironmentVariable("DEV_WEBUI");
            if(dev == "0") dev = null;
            if (!string.IsNullOrEmpty(dev))
            {
                // support vscode debugging
                // Set environment variable for remote debugging BEFORE creating WebView2
                Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=9222");
            }

            // Show off-screen to initialize WebView2, then hide
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -10000;
            Top = -10000;
            Loaded += async (s, e) =>
            {
                await InitWebView();
                Debug.WriteLine("WebView2 initialized, remote debugging available on port 9222");
                Hide();
            };
            Show(); // Triggers Loaded event
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Hide on user close; app shutdown still tears down the window.
            e.Cancel = true;
            _logViewVisible = false;
            Hide();
            base.OnClosing(e);
        }

        private async Task InitWebView()
        {
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Metatool", "WebView2");
            _webViewEnv = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await webView.EnsureCoreWebView2Async(_webViewEnv);
            if (!string.IsNullOrEmpty(dev))
            {
                Debug.WriteLine("Remote debugging enabled on port 9222. Open edge://inspect to debug.");

                webView.Source = new Uri("http://localhost:5173");
            }
            else
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var uiFolder = Path.Combine(exeDir, "_ui");
                if (Directory.Exists(uiFolder))
                {
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "metatool.local", uiFolder, CoreWebView2HostResourceAccessKind.Allow);
                    webView.Source = new Uri("https://metatool.local/index.html");
                }
            }

            webView.CoreWebView2.WebMessageReceived += WebMessageReceived;
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.WebMessageAsJson;
            Debug.WriteLine("Message from web: " + json);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var msg = doc.RootElement;
                if (msg.TryGetProperty("type", out var t))
                {
                    var type = t.GetString();
                    if (type == "close")
                    {
                        _logViewVisible = false;
                        Dispatcher.Invoke(Hide);
                    }
                    else if (type == "hotkeySelected")
                    {
                        var index = msg.GetProperty("index").GetInt32();
                        var hotkey = msg.GetProperty("hotkey");
                        var description = msg.GetProperty("description").GetString();
                        var key = hotkeys[index];
                        _selectionAction?.Invoke(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to parse web message: " + ex.Message);
            }
        }


        private TipItem[] hotkeys;
        private Action<TipItem> _selectionAction;
        public async void ShowSearch(IEnumerable<(string key, IEnumerable<string> descriptions)> tips, Action<TipItem> selectionAction = null)
        {
            hotkeys = tips.SelectMany(
                t => t.descriptions.Select(
                    d => new TipItem( t.key,d ))
            ).ToArray();
            _selectionAction = selectionAction;
            var hotkeyJson = JsonSerializer.Serialize(hotkeys);
            Debug.WriteLine("ShowSearch() called");
            _ = Dispatcher.BeginInvoke(async () =>
            {
                _logViewVisible = false;
                if (IsVisible)
                {
                    Debug.WriteLine("Window already visible, hiding");
                    Hide();
                }
                else
                {
                    Debug.WriteLine("Window not visible, making visible");
                    // Set initial size and center on screen
                    //Width = 900;
                    //Height = 300;
                    Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                    Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
                    ShowInTaskbar = true;
                    Show();
                }
                Debug.WriteLine("Calling Activate()");
                Activate();
                webView.Focus();
                await webView.EnsureCoreWebView2Async(_webViewEnv);
                Debug.WriteLine("Executing postMessage script via WebView2 postMessage");

                // Create the message object with type and hotkeys data
                var messageJson = $"{{\"type\":\"showSearch\",\"hotkeys\":{hotkeyJson}}}";

                webView.CoreWebView2.PostWebMessageAsJson(messageJson);

                // Schedule a resize after content is rendered
                //await Task.Delay(100);
                //ResizeToContent();
            });
        }

        private async void ResizeToContent()
        {
            try
            {
                // Use ExecuteScriptAsync to get the document body scrollHeight
                var height = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.scrollHeight.toString()");
                ResizeWindow(height);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                //throw;
            }
        }
        void ResizeWindow(string height)
        {
            try
            {
                if (int.TryParse(height.Trim('"'), out int contentHeight))
                {
                    Debug.WriteLine($"Content height: {contentHeight}");
                    Dispatcher.Invoke(() =>
                    {
                        //var newHeight = Math.Min(contentHeight, this.MaxHeight);

                        // Only set the Window height; let WebView2 stretch via layout so the
                        // user can still resize the window with the mouse afterwards.
                        //this.Height = newHeight;

                        //Debug.WriteLine($"Window height adjusted to: {newHeight}");
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error measuring content: {ex.Message}");

            }
        }

        public void ShowLogs(IEnumerable<LogEntryDto> bufferedLogs)
        {
            var logsJson = JsonSerializer.Serialize(bufferedLogs);
            Debug.WriteLine("ShowLogs() called");
            _ = Dispatcher.BeginInvoke(async () =>
            {
                _logViewVisible = !_logViewVisible;
                if (!_logViewVisible)
                {
                    Debug.WriteLine("Log view toggled off, hiding");
                    Hide();
                    return;
                }

                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
                ShowInTaskbar = true;
                Show();
                Activate();
                webView.Focus();
                await webView.EnsureCoreWebView2Async(_webViewEnv);

                var messageJson = $"{{\"type\":\"showLogs\",\"logs\":{logsJson}}}";
                webView.CoreWebView2.PostWebMessageAsJson(messageJson);
            });
        }

        public void SendLog(LogEntryDto entry)
        {
            if (!_logViewVisible) return;

            var entryJson = JsonSerializer.Serialize(entry);
            _ = Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var messageJson = $"{{\"type\":\"addLog\",\"log\":{entryJson}}}";
                    webView.CoreWebView2.PostWebMessageAsJson(messageJson);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to send log to WebView: {ex.Message}");
                }
            });
        }

    }
}
