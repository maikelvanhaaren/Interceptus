using System;
using System.Threading.Tasks;
using Interceptus.Internals;
using PipelineNet.ChainsOfResponsibility;

namespace Interceptus;

/// <summary>
/// Pipeline for interceptors.
/// </summary>
/// <param name="interceptorResolver"></param>
public class InterceptorPipelineAsync(IInterceptorResolver interceptorResolver)
{
    private readonly AsyncResponsibilityChain<IInterceptionContext, IMethodResult> _chain = new(new PipelineMiddlewareResolver(interceptorResolver));

    /// <summary>
    /// Add an interceptor to the pipeline.
    /// </summary>
    /// <typeparam name="TInterceptor"></typeparam>
    /// <returns></returns>
    public InterceptorPipelineAsync AddInterceptor<TInterceptor>() where TInterceptor : IInterceptor
    {
        _chain.Chain<InterceptorMiddlewareWrapper<TInterceptor>>();
        return this;
    }
	
    /// <summary>
    /// Add a function to be executed at the end of the pipeline.
    /// </summary>
    /// <param name="finallyFunc"></param>
    /// <returns></returns>
    public InterceptorPipelineAsync AddFinally(Func<IInterceptionContext, Task<IMethodResult>> finallyFunc)
    {
        _chain.Finally(finallyFunc);
        return this;
    }
	
    /// <summary>
    /// Execute the pipeline.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task<IMethodResult> ExecuteAsync(IInterceptionContext context)
    {
        return _chain.Execute(context);
    }
}