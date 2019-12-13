using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Metatool.Service
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [System.Security.SuppressUnmanagedCodeSecurity]
    public interface ICVirtualDesktopManager
    {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop(
            [In] IntPtr TopLevelWindow,
            [Out] out int OnCurrentDesktop
            );
        [PreserveSig]
        int GetWindowDesktopId(
            [In] IntPtr TopLevelWindow,
            [Out] out Guid CurrentDesktop
            );

        [PreserveSig]
        int MoveWindowToDesktop(
            [In] IntPtr TopLevelWindow,
            [MarshalAs(UnmanagedType.LPStruct)]
            [In]Guid CurrentDesktop
            );
    }

    [ComImport, Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
    public class CVirtualDesktopManager
    {

    }
    public class VirtualDesktopManager : IVirtualDesktopManager
    {
        public static VirtualDesktopManager Inst = new VirtualDesktopManager();

        public VirtualDesktopManager()
        {
            cmanager = new CVirtualDesktopManager();
            manager = (ICVirtualDesktopManager)cmanager;
        }
        ~VirtualDesktopManager()
        {
            manager = null;
            cmanager = null;
        }
        private CVirtualDesktopManager cmanager = null;
        private ICVirtualDesktopManager manager;

        public async Task<Process> GetFirstProcessOnCurrentVirtualDesktop(string exeName, Func<Process, bool> predict = null)
        {
            Process process = null;
            return (await GetProcessesOnCurrentVirtualDesktop(exeName, p =>
            {
                if (process != null) return false;
                if (predict ==null || predict(p))
                {
                    process = p;
                    return true;
                }

                return false;
            })).FirstOrDefault();
        }

        public async Task<IEnumerable<Process>> GetProcessesOnCurrentVirtualDesktop(string exeName, Func<Process, bool> predict=null)
        {
            var     processes = Process.GetProcessesByName(exeName);
            var l = new List<Process>();
            foreach (var n in processes)
            {
                if(predict!=null && !predict(n)) continue;

                var onCurrentDesktop =
                    await VirtualDesktopManager.Inst.IsOnCurrentVirtualDesktop(n.MainWindowHandle);
                if (onCurrentDesktop)
                {
                    l.Add(n);
                }
            }

            return l;
        }

        public async Task<bool> IsOnCurrentVirtualDesktop(IntPtr hWnd)
        {
            return await UiDispatcher.DispatchAsync<bool>(() =>
            {
                int hr;
                if ((hr = manager.IsWindowOnCurrentVirtualDesktop(hWnd, out var result)) != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return result != 0;
            });
        }

        public async Task<Guid> GetWindowDesktopId(IntPtr hWnd)
        {
            return await UiDispatcher.DispatchAsync<Guid>(() =>
            {
                int  hr;
                if ( (hr = manager.GetWindowDesktopId(hWnd, out var result)) != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return result;
            });
        }

        public async Task MoveWindowToDesktop(IntPtr hWnd, Guid currentDesktop)
        {
            await UiDispatcher.DispatchAsync<object>(() =>
            {
                int hr;
                if ((hr = manager.MoveWindowToDesktop(hWnd, currentDesktop)) != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return null;
            });
        }
    }
}
