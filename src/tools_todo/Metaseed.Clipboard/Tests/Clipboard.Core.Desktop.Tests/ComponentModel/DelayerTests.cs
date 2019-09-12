using System;
using System.Threading.Tasks;
using Clipboard.Core.Desktop.ComponentModel;
using Clipboard.Core.Desktop.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clipboard.Core.Desktop.Tests.ComponentModel
{
    [TestClass]
    public class DelayerTests
    {
        [TestMethod]
        public void Delayer()
        {
            var actionCalled = false;

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                var delayer = new Delayer<string>(TimeSpan.FromMilliseconds(100));
                delayer.Action += (sender, args) =>
                {
                    if (args.Data == "hello")
                    {
                        actionCalled = true;
                    }
                };

                Assert.IsFalse(actionCalled);
                delayer.ResetAndTick("hello");

                Task.Delay(100).Wait();
                DispatcherUtil.DoEvents();
                Assert.IsFalse(actionCalled);

            }, 1);

            Task.Delay(100).Wait();
            DispatcherUtil.DoEvents();
            Assert.IsTrue(actionCalled);
        }
    }
}
