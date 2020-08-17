using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metatool.Tests
{
    public class AsyncEnumerableTest
    {
        protected static IAsyncEnumerable<T> Throw<T>(Exception exception) => AsyncEnumerableEx.Throw<T>(exception);

        protected async Task AssertThrowsAsync<TException>(Task t)
            where TException : Exception
        {
            await Assert.ThrowsAsync<TException>(() => t);
        }

        protected async Task AssertThrowsAsync(Task t, Exception e)
        {
            try
            {
                await t;
            }
            catch (Exception ex)
            {
                Assert.Same(e, ex);
            }
        }

        protected Task AssertThrowsAsync<T>(ValueTask<T> t, Exception e)
        {
            return AssertThrowsAsync(t.AsTask(), e);
        }

        protected async Task NoNextAsync<T>(IAsyncEnumerator<T> e)
        {
            Assert.False(await e.MoveNextAsync());
        }

        protected async Task HasNextAsync<T>(IAsyncEnumerator<T> e, T value)
        {
            Assert.True(await e.MoveNextAsync());
            Assert.Equal(value, e.Current);
        }
    }
}
