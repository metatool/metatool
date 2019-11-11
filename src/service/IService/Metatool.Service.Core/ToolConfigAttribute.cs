using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Metatool.Service
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolConfig : Attribute
    {
        public static Type GetConfigType(Assembly assembly) =>
            GetConfigType(assembly
                .GetTypes());

        public static Type GetConfigType(IEnumerable<Type> types) =>
            types.FirstOrDefault(t => t.GetCustomAttributes(typeof(ToolConfig), true).Length > 0 && !t.IsAbstract);
    }
}
