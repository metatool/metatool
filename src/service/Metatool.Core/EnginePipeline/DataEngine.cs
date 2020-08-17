namespace Metatool.Core.EnginePipeline
 {
    public class DataEngine<TIn, TOut> : IDataEngine<TIn, TOut>
    {
        private readonly IPipeline<TIn, TOut> _pipeline;

        public DataEngine(IPipeline<TIn, TOut> pipeline)
        {
            _pipeline = pipeline;
        }
        public TOut Run(TIn stream, IContext context) => _pipeline.Flow(stream, context);
        public TOut Run(TIn stream) => _pipeline.Flow(stream, new Context());
    }
}