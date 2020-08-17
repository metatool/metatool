using System;
using Metatool.Service;

namespace Metatool.Core.EnginePipeline
{
    public class Pipeline<TIn, TLastPipeIn, TOut> : IPipeline<TIn, TOut>
    {
        protected readonly IPipe<TLastPipeIn, TOut> CurrentPipe;
        private IPipeline<TIn, TLastPipeIn> _previousPipline;
        public Pipeline(IPipe<TLastPipeIn, TOut> pipe)
        {
            CurrentPipe = pipe;
        }

        public IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TOut, TPipeOut> pipe)
        {
            return new Pipeline<TIn, TOut, TPipeOut>(pipe) { _previousPipline = this };
        }
        public IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null) where TPipe : IPipe<TOut, TPipeOut> => AddPipe(((TPipe)services?.GetService(typeof(TPipe))) ?? Services.Get<TPipe>());
        public IDataEngine<TIn, TOut> AddEngine(IDataEngine<TIn, TOut> engine = null) => engine ?? new DataEngine<TIn, TOut>(this);

        public virtual TOut Flow(TIn stream, IContext context)
        {
            var streamOut = _previousPipline.Flow(stream, context);
            return CurrentPipe.Flow(streamOut, context);
        }
    }

    internal class InitialPipeline<TIn, TOut> : Pipeline<TIn, TIn, TOut>
    {
        public InitialPipeline(IPipe<TIn, TOut> pipe) : base(pipe) { }
        public override TOut Flow(TIn stream, IContext context) => CurrentPipe.Flow(stream, context);
    }
}