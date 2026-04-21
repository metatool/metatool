using System.Diagnostics;
using System.Text.Json;

namespace Metatool.WebViewHost;

public class LogsWebViewHost : WebViewHost
{
    public LogsWebViewHost( )
    {
        Title = "Logs";
    }
    public async Task ShowLogs(IEnumerable<LogEntryDto> bufferedLogs)
    {
        var logsJson = JsonSerializer.Serialize(bufferedLogs);
        var messageJson = $"{{\"type\":\"showLogs\",\"logs\":{logsJson}}}";
        Debug.WriteLine("ShowLogs() called");

        await ShowUI(messageJson);
    }

    public void SendLog(LogEntryDto entry)
    {
        if (!ViewVisible) return;

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