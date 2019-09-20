using Metatool.Input.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metatool.Input
{
    public interface IKeyCommand<T> : ICommand<T>
    {
        IHotkey  Hotkey   { get; }
        KeyEvent KeyEvent { get; }
    }

    public class KeyCommand : Command<KeyEventArgsExt>
    {
        public KeyCommand(IHotkey hotkey, Action<KeyEventArgsExt> execute, Predicate<KeyEventArgsExt> canExecute = null)
            : base(execute, canExecute)
        {
            Hotkey = hotkey;
        }

        public IHotkey  Hotkey   { get; set; }
        public KeyEvent KeyEvent { get; set; }
    }

    public class CommandsPackage<T>
    {
        protected IEnumerable<(FieldInfo, Command<T>)> GetCommands()
        {
            var commands = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => typeof(ICommand<T>).IsAssignableFrom(f.FieldType))
                .Select(fi => (fi, fi.GetValue(this) as Command<T>));
            return commands;
        }

        public virtual void Start()
        {
            GetCommands().ToList().ForEach(c =>
            {
                var (fi, command) = c;
                if (string.IsNullOrEmpty(command.Id))
                    command.Id = GetType().FullName + fi.Name;
            });
        }
    }

    public class KeyCommandsPackage : CommandsPackage<KeyEventArgsExt>
    {
    }
}