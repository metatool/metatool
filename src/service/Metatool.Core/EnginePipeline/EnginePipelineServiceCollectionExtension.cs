using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Metatool.Core.EnginePipeline;

namespace Metatool.Core {
    public static class EnginePipelineBuilderServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelineBuilder(this IServiceCollection services){
            _ = services?? throw new ArgumentNullException(nameof(services));

            services.TryAddTransient(typeof(IEnginePipelineBuilder<>), typeof(EnginePipelineBuilder<>));
            return services;
        }
    }
}