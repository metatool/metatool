using System.Threading;
using System.Threading.Tasks;

namespace Metaseed.Input
{
    public partial class Combination : IKeyEventAsync
    {
        private SemaphoreSlim   _downEvent;
        private SemaphoreSlim   _upEvent;
        private KeyEventArgsExt _eventArgsExt;

        public async Task<KeyEventArgsExt> DownAsync(int timeout = 8888)
        {
            if (_downEvent == null) _downEvent = new SemaphoreSlim(0);
            await _downEvent.WaitAsync(timeout);
            return _eventArgsExt;
        }

        internal void OnEvent(KeyEventArgsExt arg)
        {
            _eventArgsExt = arg;
            if (arg.IsKeyDown)
            {
                _downEvent?.Release();
            }
            else
            {
                if(_upEvent==null)return;
                    _upEvent.Release();
            }
        }

        public async Task<KeyEventArgsExt> UpAsync(int timeout = 8888)
        {
            if (_upEvent == null) _upEvent = new SemaphoreSlim(0);
            await _upEvent.WaitAsync(timeout);
            return _eventArgsExt;
        }
    }
}