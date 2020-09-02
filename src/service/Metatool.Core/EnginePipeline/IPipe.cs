using System;

namespace Metatool.Core.EnginePipeline
{
    public interface IPipe<TIn, TOut>: IDisposable
    {
        TOut Flow(TIn stream, IContext context = null);
    }
    public interface IPipe<T>: IPipe<T,T>
    {
    }
}