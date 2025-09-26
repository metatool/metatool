using Metatool.MouseKeyHook.FruitMonkey;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public interface IFruitMonkey
{
    IForest Forest { get; }
    void Reset();
    void ClimbTree(KeyEventType eventType, IKeyEventArgs args);
}
