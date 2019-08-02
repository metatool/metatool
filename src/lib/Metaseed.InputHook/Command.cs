using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public class Command<T>: ICommand<T>
    {
        public Command(Action<T> execute, Predicate<T> canExecute = null)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        public virtual void Setup() { }
        public string Id { get; set; }
        public string  Description { get; set; }
        public Predicate<T> CanExecute { get; }
        public Action<T> Execute { get;}
        public bool Enabled { get; set; }
    }
}
