using System.Threading.Tasks;

namespace Metatool.Service;

public interface IMetaToolUI
{
    /// <summary>
    /// Toggles the web UI log window. Streams log entries from
    /// <see cref="Metatool.Core.Log.WebUILogSink"/> in real time while visible.
    /// </summary>
    Task  ShowLogs();
}
