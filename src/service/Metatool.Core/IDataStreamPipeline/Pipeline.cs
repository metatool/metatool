using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metatool.Core.IDataStreamPipeline
{
    public class Pipeline: IPipeline
    {
        private readonly Pipe _pipeline;

        public Pipeline(Pipe pipeline)
        {
            _pipeline = pipeline;
        }
        public async Task Flow(IDataStream context)
        {
            if (_pipeline != null)
                await _pipeline.Invoke(context, c => Task.CompletedTask);
        }
    }
}
