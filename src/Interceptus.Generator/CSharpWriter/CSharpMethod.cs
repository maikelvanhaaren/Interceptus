using System.Collections.Generic;
using System.Linq;
using Interceptus.Generator.CSharpWriter.Helpers;

namespace Interceptus.Generator.CSharpWriter;

internal class CSharpMethod : CSharpClassBody, ICSharpUnit
{
    public CSharpVisibility Visibility { get; set; }
    public string? ReturnType { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAsync { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Generics { get; set; } = [];
    public List<string> Constraints { get; set; } = [];
    public List<CSharpParameter> Parameters { get; set; } = [];
    public List<string> BodyLines { get; set; } = [];

    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.ToString()));
        var generics = Generics.Count > 0 ? $"<{string.Join(", ", Generics)}>" : "";
        var constraints = Constraints.Count > 0 ? $" where {string.Join(", ", Constraints)}" : "";

        var methodKeys = new List<string>()
            {
                Visibility.ToVisibility(),
                IsAsync ? "async" : string.Empty,
                IsOverride ? "override" : string.Empty,
                ReturnType is null ? string.Empty : $"{ReturnType}",
            }
            .Where(x => !string.IsNullOrEmpty(x));

        var methodHeader = $"{string.Join(" ", methodKeys)} {Name}{generics}({parameters}){constraints}";

        if (BodyLines.Count == 0)
        {
            return $"{methodHeader};";
        }

        if(BodyLines.Count == 1)
        {
            return $"{methodHeader} => {BodyLines[0]}";
        }


        var body = string.Join("\n", BodyLines.Select(l => $"{l}")).AppendTabToEveryLine(1);

        return $@"{methodHeader}
{{
{body}
}}
";
    }
}