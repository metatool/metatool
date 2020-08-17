using System;
using Metatool.Service;

namespace Metatool.Core.EnginePipeline
{
    public class EnginePipelineBuilder<TIn> : IEnginePipelineBuilder<TIn>
    {
        public IPipeline<TIn, TOut> AddPipe<TOut>(IPipe<TIn, TOut> pipe) => new InitialPipeline<TIn, TOut>(pipe);
        public IPipeline<TIn, TOut> AddPipe<TOut>(Func<TIn, IContext, TOut> pipe) => AddPipe(new Pipe<TIn, TOut>(pipe));
        public IPipeline<TIn, TOut> AddPipe<TOut>(Func<TIn, TOut> pipe) => AddPipe(new Pipe<TIn, TOut>(pipe));
        public IPipeline<TIn, TOut> AddPipe<TPipe, TOut>(IServiceProvider services = null) where TPipe : IPipe<TIn, TOut> => AddPipe(((TPipe)services?.GetService(typeof(TPipe)))??Services.Get<TPipe>());

    }
}