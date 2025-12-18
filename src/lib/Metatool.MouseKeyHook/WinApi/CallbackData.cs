

using System;

namespace Metatool.Input.MouseKeyHook.WinApi;

internal readonly struct CallbackData(IntPtr wParam, IntPtr lParam)
{
    public IntPtr WParam { get; } = wParam;

    public IntPtr LParam { get; } = lParam;
}