using System;
using System.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.Plugin
{
    public class Services   
    {
        internal static IServiceProvider Provider;

        public static object Get(Type serviceType)
        {
            return Provider.GetService(serviceType);
        }

        public static T Get<T>()
        {
            return (T)Provider.GetService(typeof(T));
        }

        /// <summary>
        /// Retrieve an instance of the given type from the service provider. If one is not found then instantiate it directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T GetOrCreate<T>(params object[] parameters)
        {
            var type = typeof(T);
            return (T)(Provider.GetService(type) ?? ActivatorUtilities.CreateInstance(Provider, type, parameters));
        }
        /// <summary>
        /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Create<T>(params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance<T>(Provider, parameters);
        }

        public static TOut Get<T, TOut>() 
        {
            return (TOut)Provider.GetService(typeof(T));
        }
    }
}
