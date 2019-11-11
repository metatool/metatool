using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.Utils
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddMetatoolUtils(this IServiceCollection services)
        {
            return services.AddSingleton<ICommandRunner, CommandRunner>();
        }
    }
}
