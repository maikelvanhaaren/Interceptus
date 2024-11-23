using System.Collections.Generic;
using System.Linq;
using Interceptus.Generator.CSharpWriter.Helpers;

namespace Interceptus.Generator.CSharpWriter;

internal class CSharpConstructor : CSharpClassBody, ICSharpUnit
{
    public CSharpVisibility Visibility { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CSharpParameter> Parameters { get; set; } = [];
    public string? BaseCall { get; set; }
    public List<string> BodyLines { get; set; } = [];
    
    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.ToString()));
        var constructorHeader = $"{Visibility.ToVisibility()} {Name}({parameters}){(BaseCall is null ? "" : $" : {BaseCall}")}";;
        var body = string.Join("\n", BodyLines.Select(l => $"{l}")).AppendTabToEveryLine(1);

        return $@"{constructorHeader}
{{
{body}
}}
";
    }
}