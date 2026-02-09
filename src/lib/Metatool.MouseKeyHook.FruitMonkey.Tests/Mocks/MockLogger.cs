using Microsoft.Extensions.Logging;

namespace Metatool.MouseKeyHook.FruitMonkey.Tests.Mocks;

public class MockLogger<T> : ILogger<T>
{
    public List<string> LogMessages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogMessages.Add(formatter(state, exception));
    }
}

public class MockLogger : ILogger
{
    public List<string> LogMessages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogMessages.Add(formatter(state, exception));
    }

    public void Reset() => LogMessages.Clear();
}
