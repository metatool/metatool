using System;
using System.Collections.Generic;

namespace Metatool.Core.Log;

public record LogEntry(DateTime Timestamp, string Level, string Category, string Message);

public static class WebUILogSink
{
    public const int MaxLogs = 2000;

    private static readonly LogEntry[] _buffer = new LogEntry[MaxLogs];
    private static int _head;
    private static int _count;
    private static readonly object _lock = new();

    public static event Action<LogEntry> LogReceived;

    public static void Add(LogEntry entry)
    {
        lock (_lock)
        {
            _buffer[_head] = entry;
            _head = (_head + 1) % MaxLogs;
            if (_count < MaxLogs) _count++;
        }

        LogReceived?.Invoke(entry);
    }

    public static List<LogEntry> GetBufferedLogs()
    {
        lock (_lock)
        {
            var result = new List<LogEntry>(_count);
            if (_count == 0) return result;

            var start = _count < MaxLogs ? 0 : _head;
            for (var i = 0; i < _count; i++)
            {
                result.Add(_buffer[(start + i) % MaxLogs]);
            }

            return result;
        }
    }
}
