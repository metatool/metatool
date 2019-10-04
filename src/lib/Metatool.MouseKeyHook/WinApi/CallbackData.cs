
using System;

namespace Metatool.Input.MouseKeyHook.WinApi
{
    internal struct CallbackData
    {
        public CallbackData(IntPtr wParam, IntPtr lParam)
        {
            WParam = wParam;
            LParam = lParam;
        }

        public IntPtr WParam { get; }

        public IntPtr LParam { get; }
    }
}
