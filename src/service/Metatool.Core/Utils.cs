using System.Windows;
using System.Windows.Threading;

namespace Metatool.Core;

public static class Utils
{
	internal static Dispatcher GetDispatcher(this DispatcherObject source)
	{
		//use the application's dispatcher by default
		if (Application.Current != null) return Application.Current.Dispatcher;

		//fallback for WinForms environments
		if (source.Dispatcher != null) return source.Dispatcher;

		//ultimatively use the thread's dispatcher
		return Dispatcher.CurrentDispatcher;
	}

}