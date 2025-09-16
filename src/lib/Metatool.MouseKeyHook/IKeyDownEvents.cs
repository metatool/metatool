using System;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public interface IKeyDownEvents
{
	void Down(string actionId, string description, Action<IKeyEventArgs> action);
}