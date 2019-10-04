
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal class AppMouseListener : MouseListener
    {
        public AppMouseListener()
            : base(HookHelper.HookAppMouse)
        {
        }

        protected override MouseEventExtArgs GetEventArgs(CallbackData data)
        {
            return MouseEventExtArgs.FromRawDataApp(data);
        }
    }
}
