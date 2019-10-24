using Metatool.Input;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;

namespace Metatool.ScreenHint
{
    public class ScreenHintTool : ToolBase
    {
        private readonly IKeyboard _keyboard;
        private readonly IMouse _mouse;
        private ScreenHint _screenHint;

        public ScreenHintTool( IKeyboard keyboard, IMouse mouse)         {
            _keyboard = keyboard;
            _mouse = mouse;
        }

        public override bool OnLoaded()
        {
            _screenHint = new ScreenHint(_keyboard, _mouse);
            return base.OnLoaded();
        }
    }
}
