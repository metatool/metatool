using System;

namespace Metatool.Service
{
    public interface IWindowManager
    {
        IWindow CurrentWindow { get; }
        IWindow Show(IntPtr hWnd);
    }
}