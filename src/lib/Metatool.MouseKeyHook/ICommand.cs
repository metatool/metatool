using System;

namespace Metatool.Input
{
    public interface ICommand<T>
    {
        string Description { get; set; }
        Predicate<T> CanExecute { get; }
        Action<T> Execute { get; }
        bool Enabled { get; set; }
    }
}
