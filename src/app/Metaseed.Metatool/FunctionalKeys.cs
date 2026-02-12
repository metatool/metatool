using System.Windows.Input;
using Metatool.Service;
using Metatool.Service.MouseKey;
using static Metatool.Service.MouseKey.Key;

namespace Metaseed.Metatool;

public class FunctionalKeys
{
	private readonly IKeyboard keyboard = Services.Get<IKeyboard>();

	public FunctionalKeys(IConfig<MetatoolConfig> config)
	{
		var hotKeys = config.CurrentValue.Hotkeys;

		hotKeys.TryGetValue("Exit", out var exitTrigger);
		exitTrigger ??= new HotkeyTrigger(Caps + C);
		exitTrigger.OnEvent(e =>
		{
			Services.Get<IKeyboard>().Disable = true;
			var notify = Services.Get<INotify>();
			notify.ShowMessage("MetaKeyBoard Closing...");
			Context.Exit(0);
		});

		void Restart(IKeyEventArgs e, bool isAdmin)
		{
			Services.Get<IKeyboard>().Disable = true;
			var notify = Services.Get<INotify>();
			notify.ShowMessage($"MetaKeyBoard Restarting{(isAdmin ? "(admin)" : "")}...");
			Context.Restart(0, isAdmin);
		}

		hotKeys.TryGetValue("Restart", out var restartTrigger);
		restartTrigger ??= new HotkeyTrigger(Caps + X);
		restartTrigger.OnEvent(e => Restart(e, false));

		hotKeys.TryGetValue("RestartAsAdmin", out var restartAsAdminTrigger);
		restartAsAdminTrigger ??= new HotkeyTrigger(Caps + A);
		restartAsAdminTrigger.OnEvent(e => Restart(e, true));

		hotKeys.TryGetValue("ShowCommands", out var showCommands);
		showCommands ??= new HotkeyTrigger(Caps + Question);
		showCommands.Description = "Show Commands";
		showCommands.OnEvent(e =>
		{
			// var keyboard = Services.Get<IKeyboard>();
			//Keyboard.Default.ShowTip();
			keyboard.ShowTip();
			e.Handled = true;
		});
	}

}