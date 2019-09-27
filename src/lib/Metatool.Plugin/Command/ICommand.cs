using System;

namespace Metatool.Command
{
    public interface ICommand
    {
        string Description { get; set; }
        bool   Enabled     { get; set; }
    }

    public interface ICommand<in T> : ICommand
    {
        Predicate<T> CanExecute { get; }
        Action<T>    Execute    { get; }
    }
}