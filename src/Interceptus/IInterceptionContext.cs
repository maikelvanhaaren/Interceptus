namespace Interceptus;

/// <summary>
/// Represents the context of an interception.
/// </summary>
public interface IInterceptionContext
{
    /// <summary>
    /// The method being intercepted.
    /// </summary>
    InterceptionMethod Method { get; }
    
    /// <summary>
    /// The parameters of the method.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    object GetParameter(string name);
    
    /// <summary>
    /// Set a parameter of the method.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    void SetParameter(string name, object value);
    
    /// <summary>
    /// Create a result for the method.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IMethodResult CreateResult(object? value = null);
}