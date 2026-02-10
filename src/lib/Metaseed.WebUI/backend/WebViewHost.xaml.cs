using System.Windows;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Diagnostics;
using System.IO;

namespace Metatool.WebViewHost
{
    public partial class WebViewHost : Window
    {
        private readonly string dev;

        public WebViewHost()
        {
            InitializeComponent();
            // defined in launchSettings.json
            dev = Environment.GetEnvironmentVariable("DEV_WEBUI");

            if (!string.IsNullOrEmpty(dev))
            {
                // support vscode debugging
                // Set environment variable for remote debugging BEFORE creating WebView2
                Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=9222");
            }

            // Show off-screen to initialize WebView2, then hide
            //ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            //Left = -10000;
            //Top = -10000;
            Loaded += async (s, e) =>
            {
                await InitWebView();
                Debug.WriteLine("WebView2 initialized, remote debugging available on port 9222");
                //Hide();
            };
            Show(); // Triggers Loaded event
        }

        private async Task InitWebView()
        {
            await webView.EnsureCoreWebView2Async();
            if (!string.IsNullOrEmpty(dev))
            {
                Debug.WriteLine("Remote debugging enabled on port 9222. Open edge://inspect to debug.");

                webView.Source = new Uri("http://localhost:5173");
            }
            else
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var index = Path.Combine(exeDir, "frontend", "dist", "index.html");
                if (File.Exists(index))
                    webView.Source = new Uri(index);
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
                        Dispatcher.Invoke(Close);
                    }
                    else if (type == "searchPerformed")
                    {
                        var query = msg.GetProperty("query").GetString();

                    }
                }
            }
            catch (Exception ex)
            {
				Debug.WriteLine("Failed to parse web message: " + ex.Message);
            }
        }

        public async void ShowSearch(string hotkeyJson)
        {
            Debug.WriteLine("ShowSearch() called");
            Dispatcher.Invoke(async () => {
                if (!IsVisible)
                {
                    Debug.WriteLine("Window not visible, making visible");
                    // Set initial size and center on screen
                    Width = 700;
                    Height = 300;
                    Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                    Top = (SystemParameters.PrimaryScreenHeight - Height) / 3;
                    ShowInTaskbar = true;
                    Show();
                }
                Debug.WriteLine("Calling Activate()");
                Activate();
                await webView.EnsureCoreWebView2Async();
                Debug.WriteLine("Executing postMessage script via WebView2 postMessage");

                // Create the message object with type and hotkeys data
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
                            var newHeight = Math.Min(contentHeight + 40, this.MaxHeight);

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
