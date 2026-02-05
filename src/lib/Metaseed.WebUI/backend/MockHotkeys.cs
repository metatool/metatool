using System.Text.Json;

namespace Metatool.WebViewHost
{
    public static class MockHotkeys
    {
        public sealed record Catagrey(string catagery, HotkeyItem[] children);
        public sealed record HotkeyItem(string hotkey, string description, HotkeyItem[]? children = null);

        // Returns a JSON array of mock hotkey items (pretty-printed)
        public static string GetJson()
        {
            var items = new[]
            {
                new HotkeyItem("Ctrl+Shift+F", "Open global search"),
                new HotkeyItem("Ctrl+Shift+N", "Open new workspace", [
                    new HotkeyItem("Ctrl+Shift+N", "New workspace"),
                    new HotkeyItem("B+C", "tewt sdfe")
                ]),
                new HotkeyItem("Ctrl+Alt+T", "Toggle terminal"),
                new HotkeyItem("Ctrl+P", "Quick open file", [
                    new HotkeyItem("E+D", "test 1")
                ]),
                new HotkeyItem("Alt+F4", "Close current window")
            };

            return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
