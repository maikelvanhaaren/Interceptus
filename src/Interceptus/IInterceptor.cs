using System;
using System.Threading.Tasks;

namespace Interceptus;

/// <summary>
/// Represents the context of an interception.
/// </summary>
public interface IInterceptor
{
    /// <summary>
    /// Invoke the interceptor.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public IMethodResult Invoke(IInterceptionContext context, Func<IInterceptionContext, IMethodResult> next);
    
    /// <summary>
    /// Invoke the interceptor asynchronously.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public Task<IMethodResult> InvokeAsync(IInterceptionContext context, Func<IInterceptionContext, Task<IMethodResult>> next);
}