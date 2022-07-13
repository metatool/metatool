using System;

namespace Metatool.Service
{
    public delegate void ActiveWindowChangedHandler(object sender, IntPtr hwnd);

    public interface IWindowManager
    {
        event ActiveWindowChangedHandler ActiveWindowChanged;
        IWindow CurrentWindow { get; }
        IWindow Show(IntPtr hWnd);
    }
}