using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface IKeyEventAsync
    {
        Task<KeyEventArgsExt> UpAsync(int timeout);

        Task<KeyEventArgsExt> DownAsync(int timeout);
    }
    public interface ICombination :IKeyState, IKeyEventAsync, ISequencable, ICombinable
    {
        Key TriggerKey { get; }
        IEnumerable<Key> Chord { get; }
        int ChordLength { get; }
        IEnumerable<Key> AllKeys { get; }

        bool IsAnyKey(Keys key);

    }
}