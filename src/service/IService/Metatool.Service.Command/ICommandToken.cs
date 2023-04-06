namespace Metatool.Service;

public interface ICommandToken
{
	string Id { get; set; }
}

public interface ICommandToken<T> : ICommandToken, IChangeRemove<ICommandTrigger<T>>
{
}