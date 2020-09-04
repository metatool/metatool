using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Metatool.Pipeline
{
    public class Pipeline<TIn, TPreviousPipeIn, TOut> : IPipeline<TIn, TOut>
    {
        protected IDataEngine<TIn> Engine;
        protected readonly IPipe<TPreviousPipeIn, TOut> CurrentPipe;
        private IPipeline<TIn, TPreviousPipeIn> _previousPipeline;

        public Pipeline(IPipe<TPreviousPipeIn, TOut> pipe)
        {
            CurrentPipe = pipe;
        }

        private ILogger<Pipeline<TIn, TPreviousPipeIn, TOut>> _logger;
        private ILogger<Pipeline<TIn, TPreviousPipeIn, TOut>> Logger => _logger ??= Services.Get<ILogger<Pipeline<TIn, TPreviousPipeIn, TOut>>>();

        public IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TOut, TPipeOut> pipe) =>
            new Pipeline<TIn, TOut, TPipeOut>(pipe) { _previousPipeline = this, Engine = Engine };

        public IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null)
            where TPipe : IPipe<TOut, TPipeOut> =>
            AddPipe(((TPipe)services?.GetService(typeof(TPipe))) ?? Services.Get<TPipe>() ?? throw new Exception($"could not resolve {typeof(TPipe).FullName}, make sure it is configured in the DI."));

        public virtual TOut Flow(TIn stream, IContext context = null)
        {
            context ??= new Context();
            var previousOut = _previousPipeline.Flow(stream, context);
            if (EqualityComparer<TPreviousPipeIn>.Default.Equals(previousOut, default(TPreviousPipeIn)))
                return default(TOut);
            return CurrentPipe.Flow(previousOut, context);
        }

        private IDisposable _dispose;
        public void Run(IContext context = null, Func<TOut, IDisposable> use = null) =>
         Engine.Run((stream, context) =>
         {
             var streamOut = Flow(stream, context);
             _dispose = use?.Invoke(streamOut);
             return streamOut;
         }, context ?? new Context());

        public void Dispose()
        {
            _dispose?.Dispose();
            Engine?.Dispose();
            CurrentPipe?.Dispose();
            _previousPipeline?.Dispose();
        }
    }

    internal class InitialPipeline<TIn, TOut> : Pipeline<TIn, TIn, TOut>
    {
        public InitialPipeline(IPipe<TIn, TOut> pipe) : base(pipe) { }
        public override TOut Flow(TIn stream, IContext context = null) => CurrentPipe.Flow(stream, context);
    }

    internal class EmptyPipeline<TIn> : Pipeline<TIn, TIn, TIn>
    {
        public EmptyPipeline(IDataEngine<TIn> engine) : base(default)
        {
            Engine = engine;
        }

        public override TIn Flow(TIn stream, IContext context = null) => stream;
    }
}