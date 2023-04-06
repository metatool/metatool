using System;
using Microsoft.Extensions.Logging;

namespace Metatool.Metatool;

public class SimpleConsoleLoggerProvider : ILoggerProvider
{
	public void Dispose()
	{
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new SimpleConsoleLogger(categoryName);
	}
	[ProviderAlias("SimpleConsole")]
	public class SimpleConsoleLogger : ILogger
	{
		private readonly string _categoryName;

		public SimpleConsoleLogger(string categoryName)
		{
			_categoryName = categoryName;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel)) return;
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));

			var foregroundColor = Console.ForegroundColor;
			//Console.WriteLine($"{logLevel}: {_categoryName}[{eventId.Id}]: {formatter(state, exception)}");
			if (logLevel == LogLevel.Warning)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"{formatter(state, exception)}");
				Console.ForegroundColor = foregroundColor;

			} else if (logLevel >= LogLevel.Error)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{formatter(state, exception)} \n {exception?.ToString()}");
				Console.ForegroundColor = foregroundColor;
			}
			else
			{
				Console.WriteLine($"{formatter(state, exception)}");

			}

		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel != LogLevel.None;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}
	}
}