using System.Diagnostics;
using System.Text.Json;

namespace Metatool.WebViewHost;

public class HotKeySearchWebViewHost : WebViewHost
{
    public HotKeySearchWebViewHost()
    {
        Title = "HotKeys";
    }
    private TipItem[] hotkeys;
    private Action<TipItem> _selectionAction;

    protected override void ProcessReceivedMsg(string type, JsonElement msg)
    {
        base.ProcessReceivedMsg(type, msg);
        if (type == "hotkeySelected")
        {
            var index = msg.GetProperty("index").GetInt32();
            var hotkey = msg.GetProperty("hotkey");
            var description = msg.GetProperty("description").GetString();
            var key = hotkeys[index];
            _selectionAction?.Invoke(key);
        }
    }
    public async Task ShowSearch(IEnumerable<(string key, IEnumerable<string> descriptions)> tips, Action<TipItem> selectionAction = null)
    {
        hotkeys = tips.SelectMany(
            t => t.descriptions.Select(
                d => new TipItem(t.key, d))
        ).ToArray();
        _selectionAction = selectionAction;
        var hotkeyJson = JsonSerializer.Serialize(hotkeys);
        Debug.WriteLine("ShowSearch() called");
        var messageJson = $"{{\"type\":\"showSearch\",\"hotkeys\":{hotkeyJson}}}";
        await ShowUI(messageJson);
    }
}