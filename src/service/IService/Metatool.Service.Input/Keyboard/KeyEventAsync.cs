using System.Threading;
using System.Threading.Tasks;

namespace Metatool.Service;

public interface IKeyEventAsync
{
	void OnEvent(IKeyEventArgs arg);
	Task<IKeyEventArgs> WaitAsync(int timeout);
}

public class KeyEventAsync : IKeyEventAsync
{
	private SemaphoreSlim _semaphore;
	private IKeyEventArgs _eventArgsExt;

	/// <summary>
	///  if timeout return null
	/// </summary>
	public async Task<IKeyEventArgs> WaitAsync(int timeout = -1)
	{
		_eventArgsExt = null;
		if (_semaphore == null) _semaphore = new SemaphoreSlim(0);
		await _semaphore.WaitAsync(timeout);
		return _eventArgsExt;
	}

	public void OnEvent(IKeyEventArgs arg)
	{
		if (_semaphore == null) return;

		_eventArgsExt = arg;
		_semaphore.Release();
	}
}