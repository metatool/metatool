using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.Utils
{
    public static class Main
    {
        public static IServiceCollection AddMetatoolUtils(this IServiceCollection services) =>
            services.AddSingleton<IShell, Shell>()
                .AddSingleton<IWindowManager, WindowManager>()
                .AddSingleton<IVirtualDesktopManager, VirtualDesktopManager>()
                .AddSingleton<IFileExplorer,FileExplorer>();
    }
}