namespace Interceptus.Generator.CSharpWriter;

internal class CSharpParameter : ICSharpUnit
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }

    public new string ToString()
    {
        return $"{Type} {Name}{(DefaultValue is null ? "" : $" = {DefaultValue}")}";
    }
}