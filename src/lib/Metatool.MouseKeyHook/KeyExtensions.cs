using System.Windows.Forms;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public static class KeyExtensions
{
    public static Keys ToKeys(this KeyCodes keyValue)
    {
        return (Keys)(int)keyValue;
    }

    public static KeyCodes ToKeyValues(this Keys key)
    {
        return (KeyCodes)(int)key;
    }

    public static Keys ToKeys(this Key key)
    {
        return (Keys)(KeyCodes)key;
    }
}