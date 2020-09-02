using System;

namespace Metatool.Core.EnginePipeline
{
    public interface IDataEngine<TIn>: IDisposable
    {
        void Run<TOut>(Func<TIn, IContext, TOut> flow, IContext context= null);
    }
}