namespace Metatool.Core.EnginePipeline
{
    public interface IPipe<TIn, TOut>
    {
        TOut Flow(TIn stream, IContext context);
    }
}