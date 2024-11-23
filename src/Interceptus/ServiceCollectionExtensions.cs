using System;
using Interceptus.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace Interceptus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInterceptus(this IServiceCollection services, Action<InterceptusOptions>? configure = null)
    {
        var options = new InterceptusOptions();
        configure?.Invoke(options);
        services.AddSingleton(provider => options.Resolver ?? new DefaultInterceptorResolver(provider));
        services.AddSingleton<PipelineMiddlewareResolver>();
        services.AddSingleton(typeof(InterceptorMiddlewareWrapper<>));
        return services;
    }
}