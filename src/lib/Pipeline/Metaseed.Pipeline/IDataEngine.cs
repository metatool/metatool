using System;

namespace Metatool.Pipeline
{
    public interface IDataEngine<TIn>: IDisposable
    {
        void Run<TOut>(Func<TIn, IContext, TOut> flow, IContext context);
    }
}