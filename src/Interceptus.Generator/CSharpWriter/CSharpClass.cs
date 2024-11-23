using System.Collections.Generic;
using System.Text;
using Interceptus.Generator.CSharpWriter.Helpers;

namespace Interceptus.Generator.CSharpWriter;

internal class CSharpClass : CSharpClassBody
{
    public CSharpClassType Type { get; set; }
    public CSharpVisibility Visibility { get; set; }
    public List<string> Inheritance { get; set; } = [];
    public List<string> Attributes { get; set; } = [];
    public List<string> Generics { get; set; } = [];
    public List<string> Constraints { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public List<CSharpClassBody> Body { get; set; } = [];

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var attribute in Attributes)
        {
            sb.AppendLine(attribute);
        }

        sb.Append($"{Visibility.ToVisibility()} {Type.ToString().ToLower()} {Name}");

        if (Generics.Count > 0)
        {
            sb.Append("<");
            sb.Append(string.Join(", ", Generics));
            sb.Append(">");
        }

        if (Inheritance.Count > 0)
        {
            sb.Append(" : ");
            sb.Append(string.Join(", ", Inheritance));
        }

        if (Constraints.Count > 0)
        {
            sb.Append(" where ");
            foreach (var constraint in Constraints)
            {
                sb.Append(constraint);
            }
        }

        sb.AppendLine();
        sb.AppendLine("{");

        foreach (var body in Body)
        {
            sb.AppendLine(body.ToString().AppendTabToEveryLine(1));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}