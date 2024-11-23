using System.Linq;
using System.Text;
using Interceptus.Generator.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Interceptus.Generator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax
                {
                    AttributeLists.Count: > 0
                },
                transform: (ctx, _) => GetProxyClass(ctx))
            .Where(m => m is not null);

        context.RegisterSourceOutput(classesToGenerate, static (ctx, entry) =>
        {
            if (entry is null)
            {
                return;
            }

            ctx.AddSource(entry.Value.fileName,
                SourceText.From( entry.Value.file.ToString(), Encoding.UTF8));
        });
    }

    private static (string fileName, CSharpFile file)? GetProxyClass(GeneratorSyntaxContext context)
    {
        if(context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return null;
        }

        foreach (var attributeSyntax in classDeclaration.AttributeLists.SelectMany(attributeListSyntax =>
                     attributeListSyntax.Attributes))
        {
            var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
            if (symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }
            
            var attributeFullName = attributeSymbol.ContainingType.ToDisplayString();
            if (attributeFullName != $"{Constants.Namespace}.{Constants.ClassMarkerAttribute}")
            {
                continue;
            }
            
            return Factory.CreateInterceptor(context.SemanticModel, classDeclaration);
        }
        
        return null;
    }
}