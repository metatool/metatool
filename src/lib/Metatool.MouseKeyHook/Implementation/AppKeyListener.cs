using System.Collections.Generic;
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal class AppKeyListener : KeyListener
    {
        public AppKeyListener()
            : base(HookHelper.HookAppKeyboard)
        {
        }

        protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data)
        {
            return KeyPressEventArgsExt.FromRawDataApp(data);
        }

        protected override IKeyEventArgs GetDownUpEventArgs(CallbackData data)
        {
            return KeyEventArgsExt.FromRawDataApp(data);
        }
    }
}
