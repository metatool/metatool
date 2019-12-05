

using System;
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal abstract class BaseListener : IDisposable
    {
        protected BaseListener(Subscribe subscribe)
        {
            Handle = subscribe(Callback);
        }

        protected HookResult Handle { get; set; }
        public bool Disable { get; set; }

        public void Dispose()
        {
            Handle.Dispose();
        }

        protected abstract bool Callback(CallbackData data);
    }
}
