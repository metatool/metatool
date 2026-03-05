using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Metatool;
public sealed class Timed : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly ILogger _logger;

    private Timed(string operationName, ILogger logger)
    {
        _operationName = operationName;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
        _logger.LogDebug("[START] {Operation}", _operationName);
    }

    /// <summary>Elapsed time since the operation started.</summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>Stops the timer and logs elapsed milliseconds. Called automatically at end of 'using' scope.</summary>
    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.LogDebug("[STOP]  {Operation} — {ElapsedMs:F3} ms",
            _operationName, _stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Starts a timed scope. Returns a real timer in DEBUG, a no-op in Release.
    /// Dispose (or end of 'using') stops the timer and logs the result.
    /// <example>
    /// using var _ = Timed.Start("MyOp", logger);
    /// </example>
    /// </summary>
    public static IDisposable Start(string operationName, ILogger logger)
    {
#if DEBUG
        return new Timed(operationName, logger);
#else
        return Noop.Instance;
#endif
    }

    private sealed class Noop : IDisposable
    {
        public static readonly Noop Instance = new();
        public void Dispose() { }
    }
}

public static class LoggerExtensions
{
    /// <summary>
    /// Starts a timed scope attached to this logger. Logs start/stop at Debug level.
    /// In Release builds returns a no-op — zero overhead, no allocation.
    /// <example>
    /// using var _ = _logger.Time("ProcessOrder");
    /// </example>
    /// </summary>
    public static IDisposable Time(this ILogger logger, string operationName)
        => Timed.Start(operationName, logger);
}
