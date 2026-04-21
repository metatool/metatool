using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Metatool.Core.Log;
using Metatool.Service;
using Metatool.WebViewHost;

namespace Metatool.UI;

/// <summary>
/// WebUI host for top-level Metatool UI surfaces (currently the log viewer).
/// Owns a <see cref="WebViewHost"/> window and forwards log entries
/// from <see cref="WebUILogSink"/> to the web UI in real time.
/// </summary>
public class MetaToolUI : IMetaToolUI
{
    private WebViewHost.LogsWebViewHost _webUI;

    public MetaToolUI()
    {
        Init();
    }

    private void Init()
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.BeginInvoke(Init);
            return;
        }

        _webUI = new WebViewHost.LogsWebViewHost();
        WebUILogSink.LogReceived += OnLogReceived;
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

    public async Task ShowLogs()
    {

        int counter = 0;
        if (_webUI == null)
        {
            while (_webUI == null)
            {
                Thread.Sleep(100);
                if (counter++ >= 100)
                {
                    break;
                }
            }
            Thread.Sleep(150);//additional wait to ensure the web UI is fully initialized before sending logs
        }

        var entries = WebUILogSink.GetBufferedLogs();
        var dtos = entries.Select(e => new LogEntryDto(
            e.Timestamp.ToString("HH:mm:ss.fff"),
            e.Level,
            e.Category,
            e.Message));
        await _webUI.ShowLogs(dtos);
    }
}
