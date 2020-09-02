using System;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace Metatool.Core.EnginePipeline
{
    public abstract class ObservableDataEngineBase<TIn> : IDataEngine<IObservable<TIn>>
    {
        protected abstract IObservable<TIn> ConnectToStream();
        public void Run<TOut>(Func<IObservable<TIn>, IContext, TOut> flow, IContext context = null)
        {
            var outType = typeof(TOut);
            var isIObservable = outType.IsGenericType && !outType.IsGenericTypeDefinition && outType.GetGenericTypeDefinition() == typeof(IObservable<>);
            if (!isIObservable) throw new Exception("please make sure the output of the pipeline is still an IObservable<>");

            var stream = ConnectToStream();
            flow(stream, context??new Context());
            
        }

        public void Dispose()
        {
        }
    }

    public static class ObservableSubExt
    {
        public static IDisposable Flow<T>(this IObservable<T> observable, ILogger logger=null)
        {
            return observable.Subscribe(_ => { }, e => { logger?.LogCritical(e.Message); throw e; });
        }
    }
}