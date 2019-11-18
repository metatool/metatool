using Metatool.MetaKeyboard;
using Metatool.Service;
using static Metatool.Service.Key;

namespace ConsoleApp1
{
    partial class Keyboard61 : CommandPackage
    {
        public Keyboard61(IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            SetupWinLock();
            RegisterCommands();
            var conf    = Config.Current;
            var maps    = conf.KeyMaps;
            keyboard.RegisterKeyMaps(maps);
        }

        public IKeyCommand Esc = Caps.MapOnHit(Key.Esc, e => !e.IsVirtual, false);

        public IKeyCommand ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");
    }
}