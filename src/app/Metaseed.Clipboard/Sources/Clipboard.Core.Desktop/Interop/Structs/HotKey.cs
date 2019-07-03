using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Clipboard.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HotKey
    {
        internal string Name;
        internal List<Key> Keys;
    }
}
