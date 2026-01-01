namespace Metatool.Input;

internal class SequenceHotKeyStateResetter(KeyStateTree stateTree, int timeout = 8000)
{
    private readonly Timer _timer = new Timer(o =>
    {
        if(!stateTree.IsOnRoot)
            stateTree.Reset();
    }, null, timeout, -1);

    public void Pulse()
    {
        _timer.Change(timeout, -1);
    }
}