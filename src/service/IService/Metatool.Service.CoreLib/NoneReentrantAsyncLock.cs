using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metaseed.Service.Core;
/// <summary>
/// supper simple none re-entrant locker for async code.
/// NOT FOR COMMON USAGE!!
/// </summary>
/// <remarks>
/// when use this locker, pay great attention for the codes invocation route,
/// make sure no embedded lock again.
/// </remarks>
/// <example>
///     private readonly NoneReentrantAsyncLock _configSavingLock = new();
/// in function:
///     using var async = await _configSavingLock.LockAsync();
/// or
///     using(var async = await _configSavingLock.LockAsync()) {
///
///     }
/// </example>
public class NoneReentrantAsyncLock
{
    /// <summary>
    /// note: the SemaphoreSlim does not support reentry.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();
        return new LockReleaser(_semaphore);
    }

    private sealed class LockReleaser(in SemaphoreSlim semaphore) : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = semaphore;

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}