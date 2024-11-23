using System;
using System.Threading.Tasks;
using PipelineNet.Middleware;

namespace Interceptus.Internals;

/// <summary>
/// Represents a middleware wrapper for an interceptor.
/// </summary>
/// <param name="interceptor"></param>
/// <typeparam name="TInterceptor"></typeparam>
internal class InterceptorMiddlewareWrapper<TInterceptor>(TInterceptor interceptor)
    : IMiddleware<IInterceptionContext, IMethodResult>, IAsyncMiddleware<IInterceptionContext, IMethodResult>
    where TInterceptor : IInterceptor
{
    private TInterceptor _interceptor = interceptor;

    /// <summary>
    /// Run the middleware.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public Task<IMethodResult> Run(IInterceptionContext parameter, Func<IInterceptionContext, Task<IMethodResult>> next)
    {
        return _interceptor.InvokeAsync(parameter, next);
    }

    /// <summary>
    /// Run the middleware.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public IMethodResult Run(IInterceptionContext parameter, Func<IInterceptionContext, IMethodResult> next)
    {
        return _interceptor.Invoke(parameter, next);
    }
}