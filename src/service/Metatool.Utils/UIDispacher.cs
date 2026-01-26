using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Metatool.Service;

public class UiDispatcher: IUiDispatcher
{
	internal static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

	public void Dispatch(Action action)
	{
		Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
	}
    public async Task DispatchAsync(Func<Task> action)
    {
        await Dispatcher.InvokeAsync(action);
    }

    public bool CheckAccess()
    {
        return Dispatcher.CheckAccess();
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