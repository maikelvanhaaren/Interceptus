namespace Interceptus;

/// <summary>
/// Represents a method parameter that is being intercepted.
/// </summary>
public class InterceptionMethodParameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}