using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metatool.Input
{
    public interface IKeyEventAsync
    {
        void OnEvent(IKeyEventArgs arg);
        Task<IKeyEventArgs> WaitAsync(int timeout);
    }

    public class KeyEventAsync : IKeyEventAsync
    {
        public KeyEventAsync()
        {
        }
        private SemaphoreSlim _semaphore;
        private IKeyEventArgs _eventArgsExt;
        /// <summary>
        ///  if timeout return null
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<IKeyEventArgs> WaitAsync(int timeout = -1)
        {
            _eventArgsExt = null;
            if (_semaphore == null) _semaphore = new SemaphoreSlim(0);
            await _semaphore.WaitAsync(timeout);
            return _eventArgsExt;
        }

        public void OnEvent(IKeyEventArgs arg)
        {
            if (_semaphore != null)
            {
                _eventArgsExt = arg;
                _semaphore.Release();
            }
        }

    }
}
