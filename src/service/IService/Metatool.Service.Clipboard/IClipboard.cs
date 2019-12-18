using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metatool.Service
{
    public interface IClipboard
    {
        Task<string> CopyTextAsync(CancellationToken token=default);
    }
}
