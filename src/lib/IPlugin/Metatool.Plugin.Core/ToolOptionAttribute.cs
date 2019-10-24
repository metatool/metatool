using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Metatool.Tools
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolConfig : Attribute
    {
        public static Type GetOptionType(Assembly assembly) =>
            GetOptionType(assembly
                .GetTypes());

        public static Type GetOptionType(IEnumerable<Type> types) =>
            types.FirstOrDefault(t => t.GetCustomAttributes(typeof(ToolConfig), true).Length > 0 && !t.IsAbstract);
    }
}