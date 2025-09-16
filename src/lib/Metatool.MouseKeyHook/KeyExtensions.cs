using System.Windows.Forms;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public static class KeyExtensions
{
    public static Keys ToKeys(this KeyValues keyValue)
    {
        return (Keys)(int)keyValue;
    }

    public static KeyValues ToKeyValues(this Keys key)
    {
        return (KeyValues)(int)key;
    }

    public static Keys ToKeys(this Key key)
    {
        return (Keys)(KeyValues)key;
    }
}