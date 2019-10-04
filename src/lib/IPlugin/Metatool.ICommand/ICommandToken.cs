namespace Metatool.Command
{
    public interface ICommandToken<in T> : IChangeRemove<ICommandTrigger<T>> 
    {

    }
}
