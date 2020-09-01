using System;
using System.Threading.Tasks;

namespace Metatool.Core.IDataStreamPipeline
{
    public delegate Task Pipe(IDataStream context, Func<IDataStream, Task> next);
}