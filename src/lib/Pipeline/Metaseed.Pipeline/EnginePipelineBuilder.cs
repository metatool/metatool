using Metatool.Service;
using System;

namespace Metatool.Pipeline
{
    public class EnginePipelineBuilder<TIn> : IEnginePipelineBuilder<TIn>
    {
        public IPipeline<TIn, TIn> AddEngine(IDataEngine<TIn> engine = null) =>
            new EmptyPipeline<TIn>(engine);

        public IPipeline<TIn, TIn> AddEngine<TEngine>(IServiceProvider services = null)
            where TEngine : IDataEngine<TIn> =>
            AddEngine(((TEngine) services?.GetService(typeof(TEngine))) ?? Services.Get<TEngine>() ?? throw new Exception($"could not resolve {typeof(TEngine).FullName}, make sure it is configured in the DI."));

        public IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TIn, TPipeOut> pipe) =>
            new InitialPipeline<TIn, TPipeOut>(pipe);

        public IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null)
            where TPipe : IPipe<TIn, TPipeOut>
            => AddPipe(((TPipe) services?.GetService(typeof(TPipe))) ?? Services.Get<TPipe>() ?? throw new Exception($"could not resolve {typeof(TPipe).FullName}, make sure it is configured in the DI."));
    }
}