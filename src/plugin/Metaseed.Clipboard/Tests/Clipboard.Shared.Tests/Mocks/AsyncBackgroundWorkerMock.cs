using System.Threading.Tasks;
using Clipboard.Shared.Core.Worker;

namespace Clipboard.Shared.Tests.Mocks
{
    class AsyncBackgroundWorkerMock : AsyncBackgroundWorker
    {
        protected override void DoWork()
        {
            ReportProgress(0);

            ThrowIfCanceled();
            Task.Delay(100).Wait();

            ThrowIfCanceled();
            Task.Delay(100).Wait();

            ThrowIfCanceled();
            ReportProgress(100);
        }
    }
}
