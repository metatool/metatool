using System;

namespace Metatool.Pipeline;

public interface IPipe<TIn, TOut>: IDisposable
{
	TOut Flow(TIn stream, IContext context);
}
public interface IPipe<T>: IPipe<T,T>
{
}