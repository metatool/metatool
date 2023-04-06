using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metatool.Core.IDataStreamPipeline;

public interface IPipeline
{
	Task Flow(IDataStream context);

}