using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input
{
    public class KeyboardCommandTrigger : CommandTrigger<IKeyEventArgs>
    {
        internal  MetaKey _metaKey;
        private ICommand<IKeyEventArgs> _command;

        public override ICommand<IKeyEventArgs> Command
        {
            get => _command;
            set
            {
                _command = value;
               _metaKey.ChangeDescription( _command.Description);
            }
        }

        public override void OnRemove()
        {
            _metaKey.Remove();
            base.OnRemove();
        }
    }

    public partial class Keyboard
    {
        public ICommandTrigger<IKeyEventArgs> Down(ISequenceUnit sequenceUnit)
        {
            return Event(sequenceUnit, KeyEvent.Down);
        }

        public ICommandTrigger<IKeyEventArgs> Up(ISequenceUnit sequenceUnit)
        {
            return Event(sequenceUnit, KeyEvent.Up);
        }

        private ICommandTrigger<IKeyEventArgs> Event(ISequenceUnit sequenceUnit, KeyEvent keyEvent)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger = new KeyboardCommandTrigger();
            var metaKey = Add(combination, keyEvent,
                new KeyCommand(trigger.Execute) { CanExecute = trigger.CanExecute }) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }
    }
}