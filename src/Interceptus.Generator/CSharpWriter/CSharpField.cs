using Interceptus.Generator.CSharpWriter.Helpers;

namespace Interceptus.Generator.CSharpWriter;

internal class CSharpField : CSharpClassBody, ICSharpUnit
{
    public CSharpVisibility Visibility { get; set; }
    public bool IsStatic { get; set; }
    public bool IsReadOnly { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Visibility.ToVisibility()} {(IsStatic ? "static " : "")}{(IsReadOnly ? "readonly " : "")}{Type} {Name};";
    }
}