using System.Runtime.InteropServices;

namespace Metaseed.WindowsInput.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
}
