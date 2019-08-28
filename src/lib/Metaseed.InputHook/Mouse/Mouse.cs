using Metaseed.WindowsInput;

namespace Metaseed.Input
{
    public static class Mouse
    {
        static public IMouseSimulator Simu = InputSimu.Inst.Mouse;
    }
}