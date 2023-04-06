using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Metatool.Service;

public class UiDispatcher
{
	private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

	internal static void Dispatch(Action action)
	{
		Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
	}

	internal static T Dispatch<T>(Func<T> action)
	{
		return (T) Dispatcher.Invoke(DispatcherPriority.Send, action);
	}

	internal static async Task<T> DispatchAsync<T>(Func<T> action)
	{
		var o = Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
		await o;
		return (T) (o.Result);
	}
}