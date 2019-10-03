using System.Linq;
using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input
{
    public class KeyboardCommandTrigger : CommandTrigger<IKeyEventArgs>, IKeyboardCommandTrigger
    {
        internal MetaKey _metaKey;

        public IMetaKey MetaKey => _metaKey;

        public override void OnAdd(ICommand<IKeyEventArgs> command)
        {
            _metaKey.ChangeDescription(command.Description);
            base.OnAdd(command);
        }

        public override void OnRemove(ICommand<IKeyEventArgs> command)
        {
            _metaKey.Remove();
            base.OnRemove(command);
        }
    }

    public partial class Keyboard
    {
        public IKeyboardCommandTrigger Down(ISequenceUnit sequenceUnit)
        {
            return Event(sequenceUnit, KeyEvent.Down);
        }

        public IKeyboardCommandTrigger Up(ISequenceUnit sequenceUnit)
        {
            return Event(sequenceUnit, KeyEvent.Up);
        }
        public IKeyboardCommandTrigger AllUp(ISequenceUnit sequenceUnit)
        {
            return Event(sequenceUnit, KeyEvent.AllUp);
        }
        public IKeyboardCommandTrigger Down(ISequence sequence)
        {
            return Event(sequence, KeyEvent.Down);
        }

        public IKeyboardCommandTrigger Up(ISequence sequence)
        {
            return Event(sequence, KeyEvent.Up);
        }
        public IKeyboardCommandTrigger AllUp(ISequence sequence)
        {
            return Event(sequence, KeyEvent.AllUp);
        }
        private IKeyboardCommandTrigger Event(ISequence sequence, KeyEvent keyEvent)
        {
            var seq = sequence as Sequence;
            var trigger     = new KeyboardCommandTrigger();
            var metaKey = Add(seq.ToList(), keyEvent,
                new KeyCommand(trigger.OnExecute) { CanExecute = trigger.OnCanExecute }) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }
        private IKeyboardCommandTrigger Event(ISequenceUnit sequenceUnit, KeyEvent keyEvent)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger     = new KeyboardCommandTrigger();
            var metaKey = Add(combination, keyEvent,
                new KeyCommand(trigger.OnExecute) {CanExecute = trigger.OnCanExecute}) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }
    }
}