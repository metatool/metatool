using System;
using Metatool.Service;
using Microsoft.Extensions.Options;

namespace Metaseed.Metatool.Service;

public class ToolConfig<T>: IConfig<T>
{
	private readonly IOptionsMonitor<T> _optionsMonitor;

	public ToolConfig()
	{
		_optionsMonitor = Services.Get<IOptionsMonitor<T>>();
	}

	public T CurrentValue => _optionsMonitor.CurrentValue;
	public IDisposable OnChange(Action<T, string> listener) => _optionsMonitor.OnChange(listener);
}