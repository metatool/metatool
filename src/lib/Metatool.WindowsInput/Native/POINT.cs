using System.Runtime.InteropServices;

namespace Metatool.WindowsInput.Native;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
	public int X;
	public int Y;
}