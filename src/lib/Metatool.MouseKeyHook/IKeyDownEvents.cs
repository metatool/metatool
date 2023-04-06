using System;
using Metatool.Service;

namespace Metatool.Input;

public interface IKeyDownEvents
{
	void Down(string actionId, string description, Action<IKeyEventArgs> action);
}