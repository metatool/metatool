using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Slb.Planck.Presto.ControlGateway.ServiceTests.TestTools;
using Xunit;
namespace Metatool.Pipeline.Test
{
    public class DataEnginePipelineObservableTests
    {
        class Counter
        {
            public int Count = 2;
        }

        class MultiplyPipe : IPipe<IObservable<int>, IObservable<float>>
        {
            readonly Counter _counter;
            public MultiplyPipe(Counter counter)
            {
                _counter = counter;
            }

            public IObservable<float> Flow(IObservable<int> stream, IContext context)
            {
                var scheduler = (IScheduler)context["scheduler"];
                return stream
                .Select(i => (float)i * _counter.Count++)
                .Sample(TimeSpan.FromMilliseconds(1000), scheduler);
            }
            public void Dispose()
            {
            }
        }

        [Fact()]
        public void ObservableDataPipelineWithDependencyInjection()
        {
            // arrange
            var services = new ServiceCollection().AddSingleton<Counter>().AddSingleton<MultiplyPipe>();
            var provider = services.BuildServiceProvider();

            var engine = new EnginePipelineBuilder<IObservable<int>>()
                .AddEngine()
                .AddPipe<MultiplyPipe, IObservable<float>>(provider) // 6
                .AddPipe(a => a.Select(i => (int) i + 2)) // 8
                .AddPipe<MultiplyPipe, IObservable<float>>(provider); // 24
            // act
            var scheduler = new TestScheduler();
            var observable = engine.Flow(Observable.Return(3), new Context() { { "scheduler", scheduler } });
            var observer = scheduler.Start(()=>observable, 0, 0, TimeSpan.FromMilliseconds(2000).Ticks);
            var msg = observer.Messages;
            // assert
            Assert.Equal(msg.Count, 2);
            Assert.Equal(msg[0].Value.Value, 24);
            Assert.Equal(msg[1].Value.Kind, NotificationKind.OnCompleted);
        }

        class ObservableEngine : ObservableDataEngineBase<int>
        {
            protected override IObservable<int> ConnectToStream()
            {
                return Observable.Return(3);
            }
        }

        [Fact()]
        public void ObservableDataPipelineRunWithDependencyInjection()
        {
            // arrange
            var services = new ServiceCollection().AddSingleton<Counter>().AddSingleton<MultiplyPipe>().AddSingleton<ObservableEngine>().AddSingleton<ILogger, ILoggerFake>();
            var provider = services.BuildServiceProvider();

            var engine = new EnginePipelineBuilder<IObservable<int>>()
                .AddEngine<ObservableEngine>(provider)
                .AddPipe<MultiplyPipe, IObservable<float>>(provider) // 6
                .AddPipe(a => a.Select(i => (int)i + 2)) // 8
                .AddPipe<MultiplyPipe, IObservable<float>>(provider);// 24
            // act
            var scheduler = new TestScheduler();
            engine.Run(new Context() { { "scheduler", scheduler } }, o => /*Flow()*/{
                var observer = scheduler.Start(() => o, 0, 0, TimeSpan.FromMilliseconds(2000).Ticks);
                var msg = observer.Messages;
                // assert
                Assert.Equal(msg.Count, 2);
                Assert.Equal(msg[0].Value.Value, 24);
                Assert.Equal(msg[1].Value.Kind, NotificationKind.OnCompleted);
                return null;
            });
        }
    }
}