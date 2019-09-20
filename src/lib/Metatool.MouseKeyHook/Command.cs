using System;

namespace Metatool.Input
{
    public class Command<T>: ICommand<T>
    {
        public string Id { get; set; }
        public string  Description { get; set; }
        public Predicate<T> CanExecute { get; set; }
        public Action<T> Execute { get; set; }
        public bool Enabled { get; set; }
        public override string ToString()
        {
            return Description;
        }
    }
}
