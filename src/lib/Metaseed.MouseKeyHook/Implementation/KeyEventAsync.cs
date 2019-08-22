using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metaseed.Input
{
    public interface IKeyEventAsync
    {
        void OnEvent(KeyEventArgsExt arg);
        Task<KeyEventArgsExt> WaitAsync(int timeout);
    }

    public class KeyEventAsync : IKeyEventAsync
    {
        private readonly KeyEvent _keyEvent;

        public KeyEventAsync(KeyEvent keyEvent)
        {
            _keyEvent = keyEvent;
        }
        private SemaphoreSlim   _semaphore;
        private KeyEventArgsExt _eventArgsExt;
        /// <summary>
        ///  if timeout return null
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<KeyEventArgsExt> WaitAsync(int timeout = -1)
        {
            _eventArgsExt = null;
            if (_semaphore == null) _semaphore = new SemaphoreSlim(0);
            await _semaphore.WaitAsync(timeout);
            return _eventArgsExt;
        }

        public void OnEvent(KeyEventArgsExt arg)
        {
            if (_semaphore != null && arg.KeyEvent == _keyEvent)
            {
                _eventArgsExt = arg;
                _semaphore.Release();
            }
        }

    }

    /// <summary>
    /// await TaskExt
    // .FromEvent<WebBrowserDocumentCompletedEventArgs>()
    // .WithHandlerConversion(handler => new WebBrowserDocumentCompletedEventHandler(handler))
    // .Start(
    // handler => this.webBrowser.DocumentCompleted += handler,
    // () => this.webBrowser.Navigate(@"about:blank"),
    // handler => this.webBrowser.DocumentCompleted -= handler,
    // CancellationToken.None);
    /// </summary>
    public static class TaskExt
    {
        public static EAPTask<TEventArgs, EventHandler<TEventArgs>> FromEvent<TEventArgs>()
        {
            var tcs     = new TaskCompletionSource<TEventArgs>();
            var handler = new EventHandler<TEventArgs>((s, e) => 
                tcs.TrySetResult(e));
            return new EAPTask<TEventArgs, EventHandler<TEventArgs>>(tcs, handler);
        }
    }


    public sealed class EAPTask<TEventArgs, TEventHandler>
        where TEventHandler : class
    {
        private readonly TaskCompletionSource<TEventArgs> _completionSource;
        private readonly TEventHandler                    _eventHandler;

        public EAPTask(
            TaskCompletionSource<TEventArgs> completionSource,
            TEventHandler eventHandler)
        {
            _completionSource = completionSource;
            _eventHandler     = eventHandler;
        }

        public EAPTask<TEventArgs, TOtherEventHandler> HandlerConversion<TOtherEventHandler>(
            Converter<TEventHandler, TOtherEventHandler> converter)
            where TOtherEventHandler : class
        {
            return new EAPTask<TEventArgs, TOtherEventHandler>(
                _completionSource, converter(_eventHandler));
        }

        public async Task<TEventArgs> Start(
            Action<TEventHandler> subscribe,
            Action<TEventHandler> unsubscribe,
            CancellationToken cancellationToken,
            Action trigger = null
            )
        {
            subscribe(_eventHandler);
            try
            {
                using (cancellationToken.Register(() => _completionSource.SetCanceled()))
                {
                    trigger?.Invoke();
                    return await _completionSource.Task;
                }
            }
            finally
            {
                unsubscribe(_eventHandler);
            }
        }
    }
}