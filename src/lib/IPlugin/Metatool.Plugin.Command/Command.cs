﻿using System;

namespace Metatool.Command
{
    public class Command<T> : ICommand<T>
    {
        // private readonly WeakEventSource<T> canExecutEventSource = new WeakEventSource<T>();
        // private readonly WeakEventSource<T> executEventSource    = new WeakEventSource<T>();

        public Predicate<T> CanExecute { get; set; }
        
        public Action<T> Execute     { get; set; }
        public string    Description { get; set; }
        public bool      Disabled     { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}