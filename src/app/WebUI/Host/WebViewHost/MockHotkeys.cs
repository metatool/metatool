using System.Text.Json;

namespace Metatool.WebViewHost
{
    public static class MockHotkeys
    {
        // Simple DTO for a hotkey entry
        public sealed record HotkeyItem(string hotkey, string description);

        // Returns a JSON array of mock hotkey items (pretty-printed)
        public static string GetJson()
        {
            var items = new[]
            {
                new HotkeyItem("Ctrl+Shift+F", "Open global search"),
                new HotkeyItem("Ctrl+Shift+N", "Open new workspace"),
                new HotkeyItem("Ctrl+Alt+T", "Toggle terminal"),
                new HotkeyItem("Ctrl+P", "Quick open file"),
                new HotkeyItem("Alt+F4", "Close current window")
            };

            return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
