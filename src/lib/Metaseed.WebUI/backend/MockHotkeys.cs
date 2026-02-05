using System.Text.Json;

namespace Metatool.WebViewHost
{
    public static class MockHotkeys
    {
        // Simple DTO for a hotkey entry
        public sealed record Item(string hotkey, string description, Item[]? children = null);

        // Returns a JSON array of mock hotkey items (pretty-printed)
        public static string GetJson()
        {
            var items = new[]
            {
                new Item("Ctrl+Shift+F", "Open global search"),
                new Item("Ctrl+Shift+N", "Open new workspace", new Item[]
                {
                    new Item("Ctrl+Shift+N", "New workspace"),
                    new Item("B+C", "tewt sdfe")
                }),
                new Item("Ctrl+Alt+T", "Toggle terminal"),
                new Item("Ctrl+P", "Quick open file", new Item[]
                {
                    new Item("E+D", "test 1")
                }),
                new Item("Alt+F4", "Close current window")
            };

            return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
