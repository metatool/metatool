using System;
using System.Runtime.InteropServices;
using Clipboard.Core.Desktop.Enums;

namespace Clipboard.Core.Desktop.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
}
