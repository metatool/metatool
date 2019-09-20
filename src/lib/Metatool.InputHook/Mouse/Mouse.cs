using Metatool.WindowsInput;

namespace Metatool.Input
{
    public static class Mouse
    {
        static public IMouseSimulator Simu = InputSimu.Inst.Mouse;
    }
}
