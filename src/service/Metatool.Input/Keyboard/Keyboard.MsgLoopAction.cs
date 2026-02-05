namespace Metatool.Input;

public partial class Keyboard
{
    public void Post(Action action)
    {
        _hook.Post(action);
    }

    public void Send(Action action)
    {
        _hook.Send(action);
    }
}