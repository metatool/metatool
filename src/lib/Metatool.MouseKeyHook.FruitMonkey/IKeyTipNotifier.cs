namespace Metatool.Input;

public interface IKeyTipNotifier
{
    void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips);
    void CloseKeysTip(string name);
}
