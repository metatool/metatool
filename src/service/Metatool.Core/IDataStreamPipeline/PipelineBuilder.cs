using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metatool.Core.IDataStreamPipeline
{
    public class PipelineBuilder: IPipelineBuilder
    {
        readonly List<Pipe> _pipes = new List<Pipe>();

        public IPipelineBuilder Add(Pipe pipe)
        {
            _pipes.Add(pipe);
            return this;
        }

        public IPipeline Build()
        {
            var pipe = _pipes.Aggregate(
                (first, second) =>
                    (ctx, next) =>
                        first(ctx,
                            c => second(c, next)));
            return new Pipeline(pipe);
        }

       
    }
}
