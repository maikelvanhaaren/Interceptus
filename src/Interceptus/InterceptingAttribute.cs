using System;

namespace Interceptus;

/// <summary>
/// Represents an attribute that indicates that a class should be intercepted.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class InterceptingAttribute : Attribute
{
}