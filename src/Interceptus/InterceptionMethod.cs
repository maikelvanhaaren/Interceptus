using System.Collections.Generic;

namespace Interceptus;

/// <summary>
/// Represents a method that is being intercepted.
/// </summary>
public class InterceptionMethod
{
    /// <summary>
    /// The name of the method.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;
    
    /// <summary>
    /// The return type of the method.
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;
    
    /// <summary>
    /// The parameters of the method.
    /// </summary>
    public List<InterceptionMethodParameter> Parameters { get; set; } = [];
}