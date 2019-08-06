using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Metaseed.Input
{
    public interface ICommand<T>
    {
        string Description { get; set; }
        Predicate<T> CanExecute { get; }
        Action<T> Execute { get; }
        bool Enabled { get; set; }
    }
}
