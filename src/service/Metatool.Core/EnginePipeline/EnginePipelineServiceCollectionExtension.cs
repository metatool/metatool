using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Metatool.Core.EnginePipeline {
    public static class EnginePipelineBuilderServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelineBuilder(this IServiceCollection services){
            _ = services?? throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton(typeof(IEnginePipelineBuilder<>), typeof(EnginePipelineBuilder<>));
            return services;
        }
    }
}