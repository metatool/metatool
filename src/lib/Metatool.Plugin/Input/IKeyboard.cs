using Metatool.Command;

namespace Metatool.Input
{
    public interface IKeyboard
    {
       IKeyboardCommandTrigger Down(ISequenceUnit sequenceUnit);
       IKeyboardCommandTrigger Up(ISequenceUnit sequenceUnit);
       IKeyboardCommandTrigger AllUp(ISequenceUnit sequenceUnit);
       IKeyboardCommandTrigger Down(ISequence sequenceUnit);
       IKeyboardCommandTrigger Up(ISequence sequenceUnit);
       IKeyboardCommandTrigger AllUp(ISequence sequenceUnit);
    }
}
