namespace Metatool.Core.EnginePipeline
{
    public interface IDataEngine<TIn, TOut>
    {
        IPipeline<TIn, TOut> Pipeline { set; }
        TOut Run(TIn stream = default, IContext context = null);
    }
}