using Metatool.Service;
using static Metatool.Service.Key;

namespace Metaseed.Metatool;

public class FunctionalKeys
{
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
	}

	public IKeyCommand ShowTips = (Caps + Question).OnDown(e =>
	{
		var keyboard = Services.Get<IKeyboard>();
		//Keyboard.Default.ShowTip();
		e.Handled = true;
	}, null, "Show Tips");
}