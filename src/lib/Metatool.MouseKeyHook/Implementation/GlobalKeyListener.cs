

using System.Collections.Generic;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

internal class GlobalKeyListener : KeyListener
{
	public GlobalKeyListener()
		: base(HookHelper.HookGlobalKeyboard)
	{
	}

	protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data, IKeyEventArgs arg)
	{
		return KeyPressEventArgsExt.FromRawDataGlobal(data, arg);
	}

	protected override IKeyEventArgs GetDownUpEventArgs(CallbackData data)
	{
		return KeyEventArgsExt.FromRawDataGlobal(data);
	}
}