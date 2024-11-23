using System;
using System.Collections.Generic;
using System.Linq;
using Interceptus.Generator.CSharpWriter;
using Microsoft.CodeAnalysis;

namespace Interceptus.Generator;

internal static class Extensions
{
    public static string InterceptionContextName(this IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters
            .Select(p => $"{p.Type.Name.CapitalizeFirstLetter()}_{p.Name.CapitalizeFirstLetter()}")
            .ToList();
        
        return $"{methodSymbol.Name}_InterceptionContext{(parameters.Any() ? ($"_{string.Join("_", parameters)}") : string.Empty)}";
    }
    
    public static string MethodResultName(this IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters
            .Select(p => $"{p.Type.Name.CapitalizeFirstLetter()}_{p.Name.CapitalizeFirstLetter()}")
            .ToList();
        
        return $"{methodSymbol.Name}_MethodResult{(parameters.Any() ? ($"_{string.Join("_", parameters)}") : string.Empty)}";
    }
    
    public static string ClassProxyName(this INamedTypeSymbol classSymbol)
    {
        return $"{classSymbol.Name}Proxy";
    }

    public static bool IsReturningEmptyTask(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task";
    }
    
    public static bool IsAwaitable(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.Task");
    }
    
    public static bool IsReturningSomething(this IMethodSymbol methodSymbol)
    {
        return (methodSymbol.ReturnsVoid || methodSymbol.IsReturningEmptyTask());
    }

    public static List<string> GetInterceptors(this IMethodSymbol methodSymbol)
    {
        var interceptors = new List<string>();
        
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            var attributeDefinitionName = attribute.AttributeClass?.OriginalDefinition.ToDisplayString();
            if (attributeDefinitionName is not $"{Constants.Namespace}.{Constants.MethodMarkerAttribute}<T>")
            {
                continue;
            }
            
            var attributeName = attribute.AttributeClass!.ToDisplayString();
            var interceptor = attributeName.Substring(attributeName.IndexOf("<", System.StringComparison.Ordinal) + 1);
            attributeName = interceptor.Substring(0, interceptor.Length - 1);
            
            interceptors.Add(attributeName);
        }

        return interceptors;
    }
    
    public static CSharpVisibility ToCSharpVisibility(this Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Private => CSharpVisibility.Private,
            Accessibility.Protected => CSharpVisibility.Protected,
            Accessibility.Internal => CSharpVisibility.Internal,
            Accessibility.ProtectedOrInternal => CSharpVisibility.ProtectedInternal,
            Accessibility.Public => CSharpVisibility.Public,
            _ => CSharpVisibility.Private
        };
    }

    private static string CapitalizeFirstLetter(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }
        return char.ToUpper(text[0]) + text.Substring(1);
    }

    public static string UnwrapReturnTypeTask(this IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToDisplayString();
        
        if(!returnType.Contains("<"))
        {
            return returnType;
        }
        
        
        var startIndex = returnType.IndexOf("<", System.StringComparison.Ordinal) + 1;
        var endIndex = returnType.IndexOf(">", System.StringComparison.Ordinal);
        return returnType.Substring(startIndex, endIndex - startIndex);
    }
    
    public static List<string> AddMultiple<T>(this List<string> list, List<T> items, Func<T, string> selector)
    {
        list.AddRange(items.Select(selector));
        return list;
    }

    public static MethodType GetMethodType(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsAwaitable())
        {
            return methodSymbol.IsReturningEmptyTask() ? MethodType.AsyncVoid : MethodType.AsyncValue;
        }
        
        return methodSymbol.ReturnsVoid ? MethodType.SyncVoid : MethodType.SyncValue;
    }

    internal enum MethodType
    {
        SyncVoid,
        SyncValue,
        AsyncVoid,
        AsyncValue
    }
}