namespace Interceptus;

/// <summary>
/// Represents the result of a method.
/// </summary>
public interface IMethodResult
{
    /// <summary>
    /// Get the result of the method.
    /// </summary>
    /// <returns></returns>
    public object GetResult();
    
    /// <summary>
    /// Set the result of the method.
    /// </summary>
    /// <param name="result"></param>
    public void SetResult(object result);
}