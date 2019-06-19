using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface IKey
    {
        Keys TriggerKey { get; }
        KeyEvent Type { get; }
    }
}
