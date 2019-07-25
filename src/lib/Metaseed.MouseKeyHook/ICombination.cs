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
    public interface ICombination :IKeyState, IKeyEventAsync
    {
        Keys TriggerKey { get; }
        IEnumerable<Keys> Chord { get; }
        int ChordLength { get; }
        bool Disabled { get; set; }

        ICombination With(Keys chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ICombination Control();
        ICombination Shift();
        ICombination Alt();

        ISequence Then(Keys key);
        ISequence Then(ICombination combination);

        bool IsAnyKey(Keys key);

    }
}