namespace Metatool.Command
{
    public interface ICommandToken
    {
        string Id { get; set; }
    }

    public interface ICommandToken<in T> : ICommandToken, IChangeRemove<ICommandTrigger<T>> 
    {
    }
}
