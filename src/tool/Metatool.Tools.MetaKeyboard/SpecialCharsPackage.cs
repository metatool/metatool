using System.Collections.Generic;
using Metatool.Service;
using Metatool.Service.Keyboard;
using Metatool.Service.MouseKey;

namespace Metatool.MetaKeyboard;

public class SpecialCharsPackage : CommandPackage
{
    private readonly IKeyboard _keyboard;
	private readonly OrderedDictionary<string, string> alias;

	public SpecialCharsPackage(IKeyboard keyboard, IConfig<Config> config)
    {
        _keyboard = keyboard;
        var cfg = config.CurrentValue.SpecialFrenchChars;
        alias = config.CurrentValue.KeyAliases;
        // Hotkey.Parse("shift+a");
        RegisterSpecialChars(cfg);
    }

    private void RegisterSpecialChars(ContextHotkey<string> conf)
    {
        conf.Visit((path, c) =>
        {
            if (path == "") return;// this inial

            path = _keyboard.ReplaceAlias(path, alias);

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