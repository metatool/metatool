using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Metatool.Utils.Implementation;
using Metatool.Utils.Internal;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    public class WindowManager : IWindowManager
    {
        public  IWindow CurrentWindow => new Window(PInvokes.GetForegroundWindow());

        public IWindow Show(IntPtr hWnd)
        {
            PInvokes.ShowWindowAsync(hWnd, PInvokes.SW.Show);
            PInvokes.SetForegroundWindow(hWnd);
            return new Window(hWnd);
        }
    }
}