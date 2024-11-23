using System;
using System.Linq;

namespace Interceptus.Generator.CSharpWriter.Helpers;

internal static class CSharpHelper
{
    public static string ToVisibility(this CSharpVisibility sharpVisibility)
    {
        return sharpVisibility switch
        {
            CSharpVisibility.Public => "public",
            CSharpVisibility.Internal => "internal",
            CSharpVisibility.Private => "private",
            CSharpVisibility.Protected => "protected",
            CSharpVisibility.ProtectedInternal => "protected internal",
            _ => throw new ArgumentOutOfRangeException(nameof(sharpVisibility), sharpVisibility, null)
        };
    }
    
    public static string AppendTabToEveryLine(this string str, int count)
    {
        return string.Join("\n", str.Split('\n').Select(x => new string('\t', count) + x));
    }
}