using System;
using Microsoft.Extensions.Logging;

namespace Metatool.Metatool
{
    public class CustomConsoleLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;

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
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}