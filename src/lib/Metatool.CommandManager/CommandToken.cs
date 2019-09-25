using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{

    public class CommandToken<T>: ICommandToken<T>
    {
        private readonly ICommandTrigger<T> _trigger;
        private readonly CommandManager _manager;

        public CommandToken(ICommandTrigger<T> trigger, ICommandManager manager)
        {
            _trigger = trigger;
            _manager = manager as CommandManager;
        }

        public bool Enabled
        {
            set => _manager.EnableDisable(_trigger, value);
            get => _manager.IsEnabled(_trigger);
        }

        public bool Change(T key)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            _manager.Remove(_trigger);
        }
    }
}
