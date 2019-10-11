using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metatool.Command;

namespace Metatool.Plugin
{
    public abstract class CommandPackage
    {
        protected IEnumerable<(FieldInfo, ICommandToken)> GetCommands()
        {
            var commands = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => typeof(ICommandToken).IsAssignableFrom(f.FieldType))
                .Select(fi => (fi, fi.GetValue(this) as ICommandToken));
            return commands;
        }

        protected CommandPackage()
        {
            Start();
        }

        void Start()
        {
            GetCommands().ToList().ForEach(c =>
            {
                var (fi, metaKey) = c;
                if (string.IsNullOrEmpty(metaKey.Id))
                    metaKey.Id = GetType().FullName + "." + fi.Name;
            });
        }
    }
}