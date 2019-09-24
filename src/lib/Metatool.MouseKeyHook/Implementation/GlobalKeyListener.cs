



using System.Collections.Generic;
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal class GlobalKeyListener : KeyListener
    {
        public GlobalKeyListener()
            : base(HookHelper.HookGlobalKeyboard)
        {
        }

        protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data)
        {
            return KeyPressEventArgsExt.FromRawDataGlobal(data);
        }

        protected override IKeyEventArgs GetDownUpEventArgs(CallbackData data)
        {
            return KeyEventArgsExt.FromRawDataGlobal(data);
        }
    }
}
