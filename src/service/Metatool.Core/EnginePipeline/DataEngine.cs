namespace Metatool.Core.EnginePipeline
{
    public class DataEngine<TIn, TOut> : IDataEngine<TIn, TOut>
    {
        public IPipeline<TIn, TOut> Pipeline { set; get; }

        public TOut Run(TIn stream = default, IContext context = null) => Pipeline.Flow(stream, context ?? new Context());
    }
}