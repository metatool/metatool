using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input
{
    public class KeyboardCommandTrigger : CommandTrigger<IKeyEventArgs>
    {
        internal MetaKey _metaKey;

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
            var trigger     = new KeyboardCommandTrigger();
            var metaKey = Add(combination, keyEvent,
                new KeyCommand(trigger.OnExecute) {CanExecute = trigger.OnCanExecute}) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }
    }
}