using Microsoft.Extensions.DependencyInjection;
using Metatool.Core.EnginePipeline;
using Xunit;

namespace Metatool.ServiceTests.DataEnginePipeline
{
    public class DataEnginePipelineTests
    {
        class doublePipe : IPipe<int, float>
        {
            public float Flow(int dataStream, IContext context) => dataStream * 2.0f;
            public void Dispose()
            {
            }
        }
        [Fact()]
        public void SimpleDataPipelineTest()
        {
            var engine = new EnginePipelineBuilder<int>()
                .AddEngine()
                .AddPipe((int a) => a + 1)
                .AddPipe(new doublePipe())
                .AddPipe(i => 3.0 * i)
                .AddPipe(new Pipe<double, double>(i => ++i));

            var r = engine.Flow(3);
            Assert.Equal(r, 25, 1);

            var r1 = engine.Flow(5);
            Assert.Equal(r1, 37, 1);
        }

        const string key = "multi";
        class multiplePipe : IPipe<int, float>
        {
            public float Flow(int dataStream, IContext context) => dataStream * (int)context[key];
            public void Dispose()
            {
            }
        }
        [Fact()]
        public void SimpleDataPipelineWithContextTest()
        {
            var engine = new EnginePipelineBuilder<int>()
                .AddEngine()
                .AddPipe((int a, IContext context) => a + 2)
                .AddPipe((a, context) =>
                {
                    context[key] = 3;
                    return --a;
                })
                .AddPipe(new multiplePipe())
                .AddPipe(i => 3.0 * i)
                .AddPipe(new Pipe<double, double>((i, Context) => i + (int) Context[key]));

            var r = engine.Flow(3);
            Assert.Equal(r, 39, 0);

            var r1 = engine.Flow(5, new Context());
            Assert.Equal(r1, 57, 0);
        }

        class Counter
        {
            public int Count = 2;
        }

        class multiplyPipeFromDI : IPipe<int, float>
        {
            Counter _counter;
            public multiplyPipeFromDI(Counter counter)
            {
                _counter = counter;
            }
            public float Flow(int dataStream, IContext context) => dataStream * _counter.Count++;
            public void Dispose()
            {
            }
        }

        [Fact()]
        public void SimpleDataPipelineWithDependencyInjection()
        {
            var services = new ServiceCollection().AddSingleton<Counter>().AddSingleton<multiplyPipeFromDI>();
            var provider = services.BuildServiceProvider();

            var engine = new EnginePipelineBuilder<int>()
                .AddEngine()
                .AddPipe<multiplyPipeFromDI, float>(provider) // 6
                .AddPipe(a => (int) a + 2) // 8
                .AddPipe<multiplyPipeFromDI, float>(provider); // 24

            var r = engine.Flow(3);
            Assert.Equal(r, 24, 0);
        }
    }
}