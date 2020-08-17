using System;

namespace Metatool.Core.EnginePipeline
{
    public interface IEnginePipelineBuilder<TIn>
    {
        IPipeline<TIn, TOut> AddPipe<TOut>(IPipe<TIn, TOut> pipe);
        IPipeline<TIn, TOut> AddPipe<TOut>(Func<TIn, IContext, TOut> pipe);
        IPipeline<TIn, TOut> AddPipe<TOut>(Func<TIn, TOut> pipe);
        IPipeline<TIn, TOut> AddPipe<TPipe, TOut>(IServiceProvider services = null) where TPipe : IPipe<TIn, TOut>;
    }
}