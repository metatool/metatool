using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Metatool.ServiceTests.TestTools
{
    public class ILoggerFake : ILogger
    {
        public List<(LogLevel logLevel, EventId eventId, string msg)> Messages = new List<(LogLevel, EventId, string)>();
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Messages.Add((logLevel, eventId, formatter(state, exception)));
        }
    }
}