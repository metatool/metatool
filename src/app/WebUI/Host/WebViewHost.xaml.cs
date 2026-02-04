using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            var dev = Environment.GetEnvironmentVariable("DEV_WEBUI");
            if (!string.IsNullOrEmpty(dev))
            {
                webView.Source = new Uri("http://localhost:5173");
            }
            else
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var index = System.IO.Path.Combine(exeDir, "frontend", "dist", "index.html");
                if (System.IO.File.Exists(index))
                    webView.Source = new Uri(index);
            }

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
                    if (type == "searchPerformed")
                    {
                        var query = doc.RootElement.GetProperty("query").GetString();
                        _ = PerformSearchAndShow(query);
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
                    Show();
                }
                Debug.WriteLine("Calling Activate()");
                Activate();
                await webView.EnsureCoreWebView2Async();
                Debug.WriteLine("Executing postMessage script via WebView2 postMessage");
                // Use the WebView2 API to post a message
                var json = "{\"type\":\"showSearch\"}";
                webView.CoreWebView2.PostWebMessageAsJson(json);
            });
        }

        private async Task PerformSearchAndShow(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;
            var results = await Task.Run(() => PerformSearch(query, 500));
            Dispatcher.Invoke(() => {
                var win = new SearchResults(query, results);
                win.Owner = this;
                win.Show();
            });
        }

        private List<SearchResults.Item> PerformSearch(string query, int maxResults)
        {
            var root = FindRepoRoot();
            var list = new List<SearchResults.Item>();
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return list;

            var exts = new[] { ".cs",".xaml",".svelte",".js",".ts",".md",".json",".xml",".config",".csproj",".txt",".html",".css" };
            try
            {
                var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                    .Where(f => exts.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

                foreach (var file in files)
                {
                    try
                    {
                        var lines = File.ReadAllLines(file);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                var preview = lines[i].Trim();
                                if (preview.Length > 200) preview = preview.Substring(0, 200) + "...";
                                list.Add(new SearchResults.Item { FilePath = file, LineNumber = i + 1, LinePreview = $"{i+1}: {preview}" });
                                if (list.Count >= maxResults) return list;
                            }
                        }
                    }
                    catch { /* ignore file read errors */ }
                }
            }
            catch { }

            return list;
        }

        private string FindRepoRoot()
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 6 && dir != null; i++)
                {
                    // look for solution file or .git or 'app' folder as heuristics
                    if (dir.GetFiles("*.sln").Any() || Directory.Exists(Path.Combine(dir.FullName, ".git")) || Directory.Exists(Path.Combine(dir.FullName, "app")))
                        return dir.FullName;
                    dir = dir.Parent;
                }
            }
            catch { }
            // fallback to two levels up
            try
            {
                var alt = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
                return alt;
            }
            catch { return AppDomain.CurrentDomain.BaseDirectory; }
        }
    }
}
