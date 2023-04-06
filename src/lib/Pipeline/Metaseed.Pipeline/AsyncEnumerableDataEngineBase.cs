using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metatool.Pipeline;

public abstract class AsyncEnumerableDataEngineBase<TIn> : IDataEngine<IAsyncEnumerable<TIn>>
{
	public void Dispose()
	{
	}
	protected abstract IAsyncEnumerable<TIn> ConnectToStream();
	public void Run<TOut>(Func<IAsyncEnumerable<TIn>, IContext, TOut> flow, IContext context)
	{
		var outType = typeof(TOut);
		var isAsyncEnumerable = outType.IsGenericType && !outType.IsGenericTypeDefinition && outType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);
		if (!isAsyncEnumerable) throw new Exception("please make sure the output of the pipeline is still an IAsyncEnumerable<>");

		var stream = ConnectToStream();
		flow(stream, context ?? new Context());
	}
}

public static class AsyncEnumerableExt
{
	public class CancelationDispose: IDisposable
	{
		private readonly CancellationTokenSource tokenSource;

		public CancelationDispose(CancellationTokenSource tokenSource)
		{
			this.tokenSource = tokenSource;
		}

		public void Dispose()
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
		}
	}
	public static IDisposable Flow<T>(this IAsyncEnumerable<T> asyncStream, ILogger logger = null)
	{

		var cancelSource = new CancellationTokenSource();
		Task.Run(async () =>
		{
			try
			{
				await foreach (var value in asyncStream.ConfigureAwait(false).WithCancellation(cancelSource.Token))
				{
				}
			}
			catch (Exception e)
			{
				logger.LogCritical(e.Message);
				throw;
			}
		}, cancelSource.Token);

		return new CancelationDispose(cancelSource);
	}
}