using System.Collections.Generic;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

internal class AppKeyListener : KeyListener
{
	public AppKeyListener()
		: base(HookHelper.HookAppKeyboard)
	{
	}

	protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data, IKeyEventArgs arg)
	{
		return KeyPressEventArgsExt.FromRawDataApp(data, arg);
	}

	protected override IKeyEventArgs GetDownUpEventArgs(CallbackData data)
	{
		return KeyEventArgsExt.FromRawDataApp(data);
	}
}