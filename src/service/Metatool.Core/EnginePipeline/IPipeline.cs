using System;

namespace Metatool.Core.EnginePipeline
{
    public interface IPipeline<TIn, TOut>
    {
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TOut, TPipeOut> pipe);
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TOut, IContext, TPipeOut> pipe) => AddPipe(new Pipe<TOut, TPipeOut>(pipe));
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TOut, TPipeOut> pipe) => AddPipe(new Pipe<TOut, TPipeOut>(pipe));
        IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null) where TPipe : IPipe<TOut, TPipeOut>;
        IDataEngine<TIn, TOut> AddEngine(IDataEngine<TIn, TOut> engine = null);
        TOut Flow(TIn stream, IContext context);
    }
}