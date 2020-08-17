
using System;

namespace Metatool.Core.EnginePipeline
{
    public class Pipe<TIn, TOut> : IPipe<TIn, TOut>
    {
        private readonly Func<TIn, IContext, TOut> _converterWithContext;
        private readonly Func<TIn, TOut> _converter;

        public Pipe(Func<TIn, IContext, TOut> converter) => _converterWithContext = converter;
        public Pipe(Func<TIn, TOut> converter) => _converter = converter;

        public TOut Flow(TIn dataStream, IContext context) => _converterWithContext != null ? _converterWithContext(dataStream, context) : _converter(dataStream);
    }
}