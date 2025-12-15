using Metatool.Service;
using Metatool.Service.Keyboard;
using Metatool.Service.MouseKey;

namespace Metatool.MetaKeyboard;

public class SpecialChars : CommandPackage
{
    private readonly IKeyboard _keyboard;

    public SpecialChars(IKeyboard keyboard, IConfig<Config> config)
    {
        _keyboard = keyboard;
        var cfg = config.CurrentValue.SpecialCharsPackage;
        Hotkey.Parse("shift+a");
        RegisterSpecialChars(cfg);
    }

    private void RegisterSpecialChars(ContextHotkey<string> conf)
    {
        conf.Visit((path, c) =>
        {
            if (path == "") return;// this inial 

            path = _keyboard.ReplaceAlias(path);

            if (c.Children != null)
            { 
                Hotkey.Parse(path).SetHandled().OnUp(null, description: c.Description);
            }
            else
            {
                Hotkey.Parse(path).SetHandled().OnUp(e =>
                {
                    _keyboard.Type(c.Context);
                }, description: c.Description);
            }
        });
    }
}