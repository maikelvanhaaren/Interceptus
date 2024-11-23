using System;
using Microsoft.Extensions.DependencyInjection;

namespace Interceptus.Internals;

internal class DefaultInterceptorResolver(IServiceProvider serviceProvider) : IInterceptorResolver
{
    public IInterceptor? Resolve(Type type)
    {
        return serviceProvider.GetRequiredService(type) as IInterceptor;
    }
}