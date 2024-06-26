using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Slb.Planck.Presto.ControlGateway.ServiceTests.TestTools;

public class ILoggerFake : ILogger
{
	public List<(LogLevel logLevel, EventId eventId, string msg)> Messages = new();
	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		Messages.Add((logLevel, eventId, formatter(state, exception)));
	}
}