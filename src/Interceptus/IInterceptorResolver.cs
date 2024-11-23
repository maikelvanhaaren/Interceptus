using System;

namespace Interceptus;

/// <summary>
/// Resolves interceptors.
/// </summary>
public interface IInterceptorResolver
{
    /// <summary>
    /// Resolve an interceptor.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IInterceptor? Resolve(Type type);
}