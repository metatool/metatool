using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Tests.Mocks;

public class MockKeyboardState : IKeyboardState
{
    private readonly HashSet<Key> _downKeys = [];

    public IEnumerable<Key> DownKeys => _downKeys;

    public void SetKeyDown(Key key) => _downKeys.Add(key);
    public void SetKeyDown(KeyCodes keyCode) => _downKeys.Add(new Key(keyCode));

    public void SetKeyUp(Key key) => _downKeys.Remove(key);
    public void SetKeyUp(KeyCodes keyCode) => _downKeys.RemoveWhere(k => k.Codes.Contains(keyCode));

    public void Reset() => _downKeys.Clear();

    public bool IsDown(Key key) => _downKeys.Any(k => k.Codes.Overlaps(key.Codes));

    public bool IsUp(Key key) => !IsDown(key);

    public bool IsOtherDown(Key key) => _downKeys.Any(k => !k.Codes.Overlaps(key.Codes));

    public bool AreAllDown(IEnumerable<Key> keys) => keys.All(IsDown);

    public bool AreAllUp(IEnumerable<Key> keys) => keys.All(IsUp);
}
