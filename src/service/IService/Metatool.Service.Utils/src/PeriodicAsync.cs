using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metatool.Service.Utils;

public static class IntervalAsyncExt
{
	public static async Task IntervalAsync(this TimeSpan interval, Func<Task> task,
		CancellationToken                                cancellationToken = default)
	{
		while (true)
		{
			var delayTask = Task.Delay(interval, cancellationToken).ConfigureAwait(false);
			await task();
			await delayTask;
		}
	}

	public static void Interval(this TimeSpan interval, Func<Task> task,
		CancellationToken                     cancellationToken = default)
		=> Task.Run(() => interval.IntervalAsync(task, cancellationToken), cancellationToken);
}