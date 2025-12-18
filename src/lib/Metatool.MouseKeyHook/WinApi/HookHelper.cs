

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input.MouseKeyHook.WinApi;

internal static class HookHelper
{
    private static HookProcedure _appHookProc;
    private static HookProcedure _globalHookProc;

    public static HookResult HookAppMouse(Callback callback)
    {
        return HookApp(HookIds.WH_MOUSE, callback);
    }

    public static HookResult HookAppKeyboard(Callback callback)
    {
        return HookApp(HookIds.WH_KEYBOARD, callback);
    }

    public static HookResult HookGlobalMouse(Callback callback)
    {
        // https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
        return HookGlobal(HookIds.WH_MOUSE_LL, callback);
    }

    public static HookResult HookGlobalKeyboard(Callback callback)
    {
        return HookGlobal(HookIds.WH_KEYBOARD_LL, callback);
    }

    private static HookResult HookApp(int hookId, Callback callback)
    {
        _appHookProc = (code, param, lParam) => HookProcedure(code, param, lParam, callback);

        var hookHandle = HookNativeMethods.SetWindowsHookEx(
            hookId,
            _appHookProc,
            IntPtr.Zero,
            ThreadNativeMethods.GetCurrentThreadId());

        if (hookHandle.IsInvalid)
            ThrowLastUnmanagedErrorAsException();

        return new HookResult(hookHandle, _appHookProc);
    }

    private static HookResult HookGlobal(int hookId, Callback callback)
    {
        _globalHookProc = (code, param, lParam) => HookProcedure(code, param, lParam, callback);
        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa#parameters
        var hookHandle = HookNativeMethods.SetWindowsHookEx(
            hookId,
            _globalHookProc,
            Process.GetCurrentProcess().MainModule.BaseAddress,
            0);

        if (hookHandle.IsInvalid)
            ThrowLastUnmanagedErrorAsException();

        return new HookResult(hookHandle, _globalHookProc);
    }

    // https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644984(v=vs.85)#parameters
    // https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
    private static IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam, Callback callback)
    {
        // HC_ACTION 0	
        // The wParam and lParam parameters contain information about a keystroke message.
        //HC_NOREMOVE 3
        //The wParam and lParam parameters contain information about a keystroke message,
        //and the keystroke message has not been removed from the message queue. (happened when An application called the PeekMessage function, specifying the PM_NOREMOVE flag.)
        // so no need to care this msg. otherwise duplicate event later.
        var passThrough = nCode != 0;
        if (passThrough)
            return HookNativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

        var callbackData = new CallbackData(wParam, lParam);
        var continueProcessing = callback(callbackData);

        if (!continueProcessing)
            // return a nonzero value to prevent the system from passing the message to the rest of the hook chain or the target window procedure. 
            return new IntPtr(-1);

        return HookNativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }


    private static void ThrowLastUnmanagedErrorAsException()
    {
        var errorCode = Marshal.GetLastWin32Error();
        throw new Win32Exception(errorCode);
    }
}