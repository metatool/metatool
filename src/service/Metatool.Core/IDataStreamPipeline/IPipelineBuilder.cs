using System.Threading.Tasks;

namespace Metatool.Core.IDataStreamPipeline
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder Add(Pipe pipe);
        IPipeline Build();
    }
}