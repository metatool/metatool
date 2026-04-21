using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Metatool.WebViewHost
{
    public record TipItem(string hotkey, string description);
    public record LogEntryDto(string timestamp, string level, string category, string message);

    public partial class WebViewHost : Window
    {
        private readonly string dev;
        private CoreWebView2Environment _webViewEnv;
        protected bool ViewVisible;

        public WebViewHost()
        {
            InitializeComponent();
            SourceInitialized += (_, _) => ApplyDarkTitleBar();
            // defined in launchSettings.json
            dev = Environment.GetEnvironmentVariable("DEV_WEBUI");
            if (dev == "0") dev = null;
            if (!string.IsNullOrEmpty(dev))
            {
                // support vscode debugging
                // Set environment variable for remote debugging BEFORE creating WebView2
                Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=9222");
            }

            // Show off-screen to initialize WebView2, then hide
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            var leftBackup = Left;
            var topBackup = Top;
            Left = -10000; // overcome flashing issue
            Top = -10000;
            Loaded += async (s, e) =>
            {
                await InitWebView();
                Debug.WriteLine("WebView2 initialized, remote debugging available on port 9222");
                Hide();
                //restore to normal position for when the window is shown later
                //Left = leftBackup;
                //Top = topBackup;
                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
            };
            Show(); // Triggers Loaded event
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Hide on user close; app shutdown still tears down the window.
            e.Cancel = true;
            ViewVisible = false;
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
                if (!msg.TryGetProperty("type", out var t))
                    return;

                var type = t.GetString();
                ProcessReceivedMsg(type, msg);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to parse web message: " + ex.Message);
            }
        }

        protected virtual void ProcessReceivedMsg(string type, JsonElement msg)
        {
            if (type == "close")
            {
                ViewVisible = false;
                Dispatcher.Invoke(Hide);
            }
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

        public async Task ShowUI(string messageJson)
        {
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.BeginInvoke(async () => await ShowUI(messageJson));
                return;
            }

            //while (!IsLoaded)
            //{
            //    await Task.Delay(100);
            //}

            if (WindowState == WindowState.Minimized)
            {
                Debug.WriteLine("Window is minimized, restoring");
                WindowState = WindowState.Normal;
                return;
            }
            ViewVisible = !ViewVisible;
            if (!ViewVisible) //IsVisible
            {
                Debug.WriteLine("Log view toggled off, hiding");
                Hide();
                return;
            }

            // Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            // Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
            ShowInTaskbar = true;
            Show();
            Activate();
            webView.Focus();
            await webView.EnsureCoreWebView2Async(_webViewEnv);
            webView.CoreWebView2.PostWebMessageAsJson(messageJson);
            // Schedule a resize after content is rendered in webUI
            //await Task.Delay(100);
            //ResizeToContent();
        }

    }
}
