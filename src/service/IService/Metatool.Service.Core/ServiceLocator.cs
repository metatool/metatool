using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    public interface IServiceProviderDisposable : IServiceProvider, IDisposable
    {
    }


    public static class Services
    {
        public class ChildServiceProvider : IServiceProviderDisposable
        {
            private readonly IServiceProvider _child;
            private readonly IServiceProvider _parent;

            public ChildServiceProvider(IServiceProvider parent, IServiceProvider child)
            {
                _parent = parent;
                _child  = child;
            }

            public ChildServiceProvider(IServiceProvider parent, IServiceCollection services)
            {
                _parent = parent;
                _child  = services.BuildServiceProvider();
            }

            public void Dispose()
            {
                _provider = _parent;
                (_child as IDisposable)?.Dispose();
            }

            public object GetService(Type serviceType)
            {
                return _child.GetService(serviceType) ?? _parent.GetService(serviceType);
            }
        }

        static ILogger commonLogger;
        public static ILogger CommonLogger => commonLogger??= GetOrCreate<ILogger<Object>>();

        static IServiceProvider _provider;

        internal static void SetDefaultProvider(IServiceProvider provider) => _provider = provider;

        internal static IServiceProviderDisposable AddServices(IServiceCollection services)
        {
            var p = new ChildServiceProvider(_provider, services);
            _provider = p;
            return p;
        }

        public static object Get(Type serviceType)
        {
            return _provider.GetService(serviceType);
        }

        public static T Get<T>()
        {
            return (T) Get(typeof(T));
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
            // Note: the CreateInstance not take account the sub providers
            return (T) (Get(type) ?? ActivatorUtilities.CreateInstance(_provider, type, parameters));
        }

        /// <summary>
        /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Create<T>(params object[] parameters)
        {
            // Note: the CreateInstance not take account the sub providers
            return ActivatorUtilities.CreateInstance<T>(_provider, parameters);
        }

        /// <summary>
        /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="instanceType">The type to activate</param>
        /// <param name="parameters">Constructor arguments not provided by the <paramref name="provider"/>.</param>
        /// <returns>An activated object of type instanceType</returns>
        public static T Create<T>(Type instanceType, params object[] parameters)
        {
            // Note: the CreateInstance not take account the sub providers
            return (T) ActivatorUtilities.CreateInstance(_provider, instanceType, parameters);
        }

        public static T Create<T>(this IServiceProviderDisposable provider, Type instanceType,
            params object[] parameters)
        {
            return (T) ActivatorUtilities.CreateInstance(_provider, instanceType, parameters);
        }

        public static TImpl Get<T, TImpl>()
            where TImpl : class, T

        {
            return _provider.Get<T, TImpl>();
        }

        public static TImpl Get<T, TImpl>(this IServiceProvider provider)
            where TImpl : class, T
        {
            return (TImpl) Get(typeof(T));
        }

        public static TImpl GetOrCreate<T, TImpl>(this IServiceProvider provider, params object[] parameters) 
            where TImpl : class, T
        {
            return (TImpl)GetOrCreate<T>(parameters);
        }
    }
}
