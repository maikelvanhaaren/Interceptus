using System;
using PipelineNet.MiddlewareResolver;

namespace Interceptus.Internals;

/// <summary>
/// Resolves middleware.
/// </summary>
/// <param name="interceptorResolver"></param>
internal class PipelineMiddlewareResolver(IInterceptorResolver interceptorResolver) : IMiddlewareResolver
{
    public MiddlewareResolverResult Resolve(Type type)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(InterceptorMiddlewareWrapper<>))
        {
            return new MiddlewareResolverResult()
            {
                Middleware = null,
                IsDisposable = false
            };
        }
        
        var interceptor = interceptorResolver.Resolve(type.GenericTypeArguments[0]);
        if (interceptor == null)
        {
            return new MiddlewareResolverResult()
            {
                Middleware = null,
                IsDisposable = false
            };
        }

        var instance = Activator.CreateInstance(type, interceptor);
        
        return new MiddlewareResolverResult()
        {
            Middleware = instance,
            IsDisposable = false
        };
    }
}