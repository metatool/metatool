namespace Metatool.Core.EnginePipeline
{
    public interface IDataEngine<TIn, TOut>
    {
        TOut Run(TIn stream, IContext context);
        TOut Run(TIn stream);
    }
}