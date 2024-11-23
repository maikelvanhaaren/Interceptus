using System;

namespace Interceptus;

/// <summary>
/// Represents an attribute that indicates that a method should be intercepted.
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InterceptorAttribute<T> : Attribute where T : IInterceptor
{
    
}