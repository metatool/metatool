using System.Collections.Generic;

namespace Metaseed;

/// <summary>
/// Metaseed.DebugState.Watcher
/// </summary>
public static class DebugState
{
    public static Dictionary<string, object> Watcher = new();
    public static void Add(string name, object obj)
    {
#if DEBUG
        Watcher[name] = obj;
#endif
    }
}