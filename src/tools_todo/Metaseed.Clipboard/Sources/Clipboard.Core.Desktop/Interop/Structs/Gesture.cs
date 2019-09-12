using System.Runtime.InteropServices;

namespace Clipboard.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Gesture
    {
        internal string Name;
        internal int[,] CheckPoinstArray;
    }
}
