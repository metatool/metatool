using System;
using Microsoft.Extensions.Logging;

namespace Metatool.Core.Log;

[ProviderAlias("WebUI")]
public class WebUILoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new WebUILogger(categoryName);

    public void Dispose() { }

    private class WebUILogger(string categoryName) : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);
            if (exception != null)
                message += $"\n{exception}";

            var entry = new LogEntry(
                DateTime.Now,
                logLevel.ToString(),
                categoryName,
                message);

            WebUILogSink.Add(entry);
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
