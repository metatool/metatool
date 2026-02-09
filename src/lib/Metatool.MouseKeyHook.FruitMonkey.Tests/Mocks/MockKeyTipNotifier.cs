using Metatool.Input;

namespace Metatool.MouseKeyHook.FruitMonkey.Tests.Mocks;

public class MockKeyTipNotifier : IKeyTipNotifier
{
    public List<(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips)> ShownTips { get; } = [];
    public List<string> ClosedTips { get; } = [];

    public void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips)
    {
        ShownTips.Add((name, tips));
    }

    public void CloseKeysTip(string name)
    {
        ClosedTips.Add(name);
    }

    public void Reset()
    {
        ShownTips.Clear();
        ClosedTips.Clear();
    }
}
