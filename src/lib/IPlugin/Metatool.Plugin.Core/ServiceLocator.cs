using System;
using System.Runtime;

namespace Metatool.Plugin
{
    public class Services   
    {
        internal static IServiceProvider Provider;

        // public static object Get(Type serviceType)
        // {
        //     return Provider.GetService(serviceType);
        // }

        public static T Get<T>()
        {
            return (T)Provider.GetService(typeof(T));
        }

        public static TOut Get<T, TOut>() 
        {
            return (TOut)Provider.GetService(typeof(T));
        }
    }
}
