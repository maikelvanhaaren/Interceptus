using System.Collections.Generic;
using System.Text;

namespace Interceptus.Generator.CSharpWriter;

public class CSharpRegion : CSharpClassBody
{
    public string Name { get; set; } = string.Empty;
    public List<CSharpClassBody> Body { get; set; } = [];
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"#region {Name}");
        sb.AppendLine();
        
        foreach (var body in Body)
        {
            sb.AppendLine(body.ToString());
        }
        
        sb.AppendLine();
        sb.AppendLine("#endregion");
        
        return sb.ToString();
    }
}