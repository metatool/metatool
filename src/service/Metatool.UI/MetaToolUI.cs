using System.Linq;
using System.Windows;
using Metatool.Core.Log;
using Metatool.Service;
using Metatool.WebViewHost;

namespace Metatool.UI;

/// <summary>
/// WebUI host for top-level Metatool UI surfaces (currently the log viewer).
/// Owns a <see cref="WebViewHost.WebViewHost"/> window and forwards log entries
/// from <see cref="WebUILogSink"/> to the web UI in real time.
/// </summary>
public class MetaToolUI : IMetaToolUI
{
    private WebViewHost.WebViewHost _webUI;

    public MetaToolUI()
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _webUI = new WebViewHost.WebViewHost();
            WebUILogSink.LogReceived += OnLogReceived;
        });
    }

    private void OnLogReceived(LogEntry entry)
    {
        if (_webUI == null) return;

        var dto = new LogEntryDto(
            entry.Timestamp.ToString("HH:mm:ss.fff"),
            entry.Level,
            entry.Category,
            entry.Message);
        _webUI.SendLog(dto);
    }

    public void ShowLogs()
    {
        if (_webUI == null) return;

        var entries = WebUILogSink.GetBufferedLogs();
        var dtos = entries.Select(e => new LogEntryDto(
            e.Timestamp.ToString("HH:mm:ss.fff"),
            e.Level,
            e.Category,
            e.Message));
        _webUI.ShowLogs(dtos);
    }
}
