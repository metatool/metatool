using System;
using System.Runtime;

namespace Metatool.Plugin
{
    public class ServiceLocator   
    {
        internal static IServiceProvider Current;

        public static object GetService(Type serviceType)
        {
            return Current.GetService(serviceType);
        }

        public static T GetService<T>()
        {
            return (T)Current.GetService(typeof(T));
        }

        public static TOut GetService<T, TOut>() 
        {
            return (TOut)Current.GetService(typeof(T));
        }
    }
}
