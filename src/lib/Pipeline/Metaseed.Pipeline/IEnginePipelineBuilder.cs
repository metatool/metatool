using System;

namespace Metatool.Pipeline;

public interface IEnginePipelineBuilder<TIn>
{
	IPipeline<TIn, TIn> AddEngine(IDataEngine<TIn> engine = null);
	IPipeline<TIn, TIn> AddEngine<TEngine>(IServiceProvider services = null) where TEngine : IDataEngine<TIn>;

	IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(IPipe<TIn, TPipeOut> pipe);
	IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TIn, IContext, TPipeOut> pipe) => AddPipe(new Pipe<TIn, TPipeOut>(pipe));
	IPipeline<TIn, TPipeOut> AddPipe<TPipeOut>(Func<TIn, TPipeOut> pipe) => AddPipe(new Pipe<TIn, TPipeOut>(pipe));
	IPipeline<TIn, TPipeOut> AddPipe<TPipe, TPipeOut>(IServiceProvider services = null) where TPipe : IPipe<TIn, TPipeOut>;
}