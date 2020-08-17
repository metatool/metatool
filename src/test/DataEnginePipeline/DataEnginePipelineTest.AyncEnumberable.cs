using Xunit;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Metatool.Tests;
using Microsoft.Extensions.DependencyInjection;
using Metatool.Service;

namespace Metatool.Core.EnginePipeline.Tests
{
    public class DataEnginePipelineAsyncEnumerableTests : AsyncEnumerableTest
    {
        class Counter
        {
            public int Count = 2;
        }

        class MultiplyPipe : IPipe<IAsyncEnumerable<int>, IAsyncEnumerable<float>>
        {
            readonly Counter _counter;
            private readonly ILogger _logger;

            public MultiplyPipe(Counter counter, ILogger logger)
            {
                _counter = counter;
                _logger = logger;
            }

            public IAsyncEnumerable<float> Flow(IAsyncEnumerable<int> stream, IContext context)
            {
                return stream
                .Select(i => (float)i * _counter.Count++)
                .SelectAwait(async i => await new ValueTask<float>(i))
                .Do(f => _logger.LogInformation(f.ToString(CultureInfo.InvariantCulture)));
            }
        }

        [Fact()]
        public async void AsyncEnumerableDataPipelineWithDependencyInjection()
        {
            // arrange
            var services = new ServiceCollection().AddSingleton<Counter>().AddSingleton<MultiplyPipe>().AddSingleton<ILogger, ILoggerFake>();
            var provider = services.BuildServiceProvider();
            var engine = new EnginePipelineBuilder<IAsyncEnumerable<int>>()
                .AddPipe<MultiplyPipe, IAsyncEnumerable<float>>(provider) // 6
                .AddPipe(a => a.Select(i => (int)i + 2)) // 8
                .AddPipe<MultiplyPipe, IAsyncEnumerable<float>>(provider) // 24
                .AddEngine();
            // act
            var asyncEnumerable = engine.Run(new[] { 3, 4, 5 }.ToAsyncEnumerable());
            // assert
            var e = asyncEnumerable.GetAsyncEnumerator();
            await HasNextAsync(e, 24);
            await HasNextAsync(e, 90);
            await HasNextAsync(e, 224);
            await NoNextAsync(e);
            var logger = provider.Get<ILogger, ILoggerFake>();
            Assert.Equal(logger.Messages.Count, 6);

            var rEnumerator =new[] { 234.0f, 462, 806 }.GetEnumerator();
            await foreach(var value in asyncEnumerable) {
                Assert.True(rEnumerator.MoveNext());
                Assert.Equal( value, rEnumerator.Current);
            }
        }

    }
}