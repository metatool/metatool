using System;
using System.Diagnostics;

namespace Metatool.Input.MouseKeyHook;
/// <summary>
/// Note: this way not work well, i.e. every key press at the up event that key is down.
/// var clearStuckKeysScheduler = new IdleActionScheduler(KeyboardState.ClearStuckKeys);
/// _eventSource.KeyUp += (sender, args) =>
///{
///    clearStuckKeysScheduler.CheckIdle();
///        ...
///    clearStuckKeysScheduler.MayExecute();
///};
/// </summary>
/// <param name="action"></param>
/// <param name="idleTimeoutMs"></param>
public class IdleActionScheduler(Action action, int idleTimeoutMs = 8000)
{
    private readonly Stopwatch _stopwatch = new();
    private bool _toExecute;
    public void CheckIdle()
    {
        if (_toExecute)
        {
            _toExecute = false;
        }

        if (_stopwatch.ElapsedMilliseconds >= idleTimeoutMs)
        {
            _toExecute = true;
        }

        _stopwatch.Restart();
    }

    public void MayExecute()
    {
        if (_toExecute)
        {
            action();
            _toExecute = false;
        }
    }
}