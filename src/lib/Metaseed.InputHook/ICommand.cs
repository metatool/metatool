using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Metaseed.Input
{
    public interface ICommand<T>
    {
        string Id { get; }
        string Description { get; }
        void Setup();
        Predicate<T> CanExecute { get; }
        Action<T> Execute { get; }
        bool Enabled { get; set; }
    }
}
