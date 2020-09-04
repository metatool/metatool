using System;

namespace Metatool.Pipeline
{
    public interface IPipeline<TIn, TOut>: IDisposable
    {
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TOut, TPipeOut> pipe);
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TOut, IContext, TPipeOut> pipe) => AddPipe(new Pipe<TOut, TPipeOut>(pipe));
        IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TOut, TPipeOut> pipe) => AddPipe(new Pipe<TOut, TPipeOut>(pipe));
        IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null) where TPipe : IPipe<TOut,  TPipeOut>;

        // just call this when we have engine added, otherwise just call the Flow function
        void Run(IContext context = null, Func<TOut, IDisposable> use = null);
        TOut Flow(TIn stream, IContext context=null);
    }
}