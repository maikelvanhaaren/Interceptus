using System;
using System.Collections.Generic;
using System.Linq;
using Interceptus.Generator.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Interceptus.Generator;

internal class Factory
{
    public static (string fileName, CSharpFile file)? CreateInterceptor(SemanticModel semanticModel,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        var proxyClass = new CSharpClass()
        {
            Name = classSymbol.ClassProxyName(),
            Inheritance = [classSymbol.Name],
            Visibility = classSymbol.DeclaredAccessibility.ToCSharpVisibility(),
            Type = CSharpClassType.Class,
            Body =
            [
                new CSharpField()
                {
                    Visibility = CSharpVisibility.Private,
                    IsReadOnly = true,
                    Type = $"{Constants.Namespace}.{Constants.InterceptorResolverInterface}",
                    Name = "_interceptorResolver"
                }
            ]
        };

        proxyClass.Body.AddRange(GetConstructors(classSymbol));
        proxyClass.Body.AddRange(GetInterceptingMethods(classSymbol));

        return ($"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}.g.cs",
            new CSharpFile()
            {
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                Classes = [proxyClass]
            });
    }

    private static List<CSharpConstructor> GetConstructors(INamedTypeSymbol classSymbol)
    {
        var constructors = new List<CSharpConstructor>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.MethodKind != MethodKind.Constructor)
            {
                continue;
            }

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var parameters = methodSymbol.Parameters
                .Select(p => new CSharpParameter()
                {
                    Type = p.Type.ToDisplayString(),
                    Name = p.Name
                })
                .Concat([
                    new CSharpParameter()
                    {
                        Type = $"{Constants.Namespace}.{Constants.InterceptorResolverInterface}",
                        Name = "interceptorResolver"
                    }
                ])
                .ToList();

            constructors.Add(new CSharpConstructor()
            {
                Visibility = CSharpVisibility.Public,
                Name = classSymbol.ClassProxyName(),
                Parameters = parameters,
                BaseCall = $"base({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))})",
                BodyLines = ["_interceptorResolver = interceptorResolver;"]
            });
        }

        return constructors;
    }

    private static List<CSharpClassBody> GetInterceptingMethods(INamedTypeSymbol classSymbol)
    {
        var classBodies = new List<CSharpClassBody>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public ||
                methodSymbol.MethodKind == MethodKind.Constructor)
            {
                continue;
            }

            if (!methodSymbol.IsVirtual || methodSymbol.IsStatic)
            {
                continue;
            }

            classBodies.AddRange(GetInterceptingMethod(methodSymbol));
        }

        return classBodies;
    }

    private static List<CSharpClassBody> GetInterceptingMethod(IMethodSymbol methodSymbol)
    {
        var classBodies = new List<CSharpClassBody>();

        var interceptors = methodSymbol.GetInterceptors();

        if (interceptors.Count == 0)
        {
            return classBodies;
        }

        // Context class
        classBodies.Add(GetContextClass(methodSymbol));

        // Method result class
        classBodies.Add(GetMethodResultClass(methodSymbol));

        // Method body
        var newMethod = new CSharpMethod()
        {
            Visibility = CSharpVisibility.Public,
            IsOverride = true,
            IsAsync = methodSymbol.IsAwaitable(),
            ReturnType = methodSymbol.ReturnType.ToDisplayString(),
            Name = methodSymbol.Name,
            Parameters = methodSymbol.Parameters
                .Select(p => new CSharpParameter()
                {
                    Type = p.Type.ToDisplayString(),
                    Name = p.Name
                })
                .ToList(),
            BodyLines = []
        };

        var methodType = methodSymbol.GetMethodType();
        var baseMethodCall = "base." + methodSymbol.Name + "(" +
                             string.Join(", ", methodSymbol.Parameters.Select(x => $"context.{x.Name}")) + ");";
        var contextNotCorrectTypeMessage = $"Context is not of type {methodSymbol.InterceptionContextName()}";
        var resultNotCorrectTypeMessage = $"Result is not of type {methodSymbol.MethodResultName()}";

        switch (methodType)
        {
            case Extensions.MethodType.SyncVoid:
                newMethod.BodyLines.AddRange(new List<string>()
                {
                    $"var pipeline = new {Constants.Namespace}.{Constants.PipelineClass}(_interceptorResolver)",
                    string.Join("\n", interceptors.Select(x => $"   .AddInterceptor<{x}>()")),
                    "   .AddFinally((ctx) => ",
                    "   {",
                    "       if(ctx is not " + methodSymbol.InterceptionContextName() + " context)",
                    "       {",
                    "           throw new InvalidOperationException(\"" + contextNotCorrectTypeMessage + "\");",
                    "       }",
                    "",
                    "       " + baseMethodCall,
                    "",
                    "       return new " + methodSymbol.MethodResultName() + "();",
                    "   });",
                    "",
                    $"var ctx = new {methodSymbol.InterceptionContextName()}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))});",
                    "",
                    "pipeline.Execute(ctx);",
                });
                break;
            case Extensions.MethodType.SyncValue:
                newMethod.BodyLines.AddRange(new List<string>()
                {
                    $"var pipeline = new {Constants.Namespace}.{Constants.PipelineClass}(_interceptorResolver)",
                    string.Join("\n", interceptors.Select(x => $"   .AddInterceptor<{x}>()")),
                    "   .AddFinally((ctx) => ",
                    "   {",
                    "       if(ctx is not " + methodSymbol.InterceptionContextName() + " context)",
                    "       {",
                    "           throw new InvalidOperationException(\"" + contextNotCorrectTypeMessage + "\");",
                    "       }",
                    "",
                    "       var result = " + baseMethodCall,
                    "",
                    "       return new " + methodSymbol.MethodResultName() + "(result);",
                    "   });",
                    "",
                    $"var ctx = new {methodSymbol.InterceptionContextName()}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))});",
                    "",
                    "var result = pipeline.Execute(ctx);",
                    "if(result is not " + methodSymbol.MethodResultName() + " methodResult)",
                    "{",
                    "    throw new InvalidOperationException(\"" + resultNotCorrectTypeMessage + "\");",
                    "}",
                    "",
                    "return methodResult.Result;"
                });
                break;
            case Extensions.MethodType.AsyncVoid:
                newMethod.BodyLines.AddRange(new List<string>()
                {
                    $"var pipeline = new {Constants.Namespace}.{Constants.PipelineAsyncClass}(_interceptorResolver)",
                    string.Join("\n", interceptors.Select(x => $"   .AddInterceptor<{x}>()")),
                    "   .AddFinally(async (ctx) => ",
                    "   {",
                    "       if(ctx is not " + methodSymbol.InterceptionContextName() + " context)",
                    "       {",
                    "           throw new InvalidOperationException(\"" + contextNotCorrectTypeMessage + "\");",
                    "       }",
                    "",
                    "       await " + baseMethodCall,
                    "",
                    "       return new " + methodSymbol.MethodResultName() + "();",
                    "   });",
                    "",
                    $"var ctx = new {methodSymbol.InterceptionContextName()}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))});",
                    "",
                    "await pipeline.ExecuteAsync(ctx);",
                });
                break;
            case Extensions.MethodType.AsyncValue:
                newMethod.BodyLines.AddRange(new List<string>()
                {
                    $"var pipeline = new {Constants.Namespace}.{Constants.PipelineAsyncClass}(_interceptorResolver)",
                    string.Join("\n", interceptors.Select(x => $"   .AddInterceptor<{x}>()")),
                    "   .AddFinally(async (ctx) => ",
                    "   {",
                    "       if(ctx is not " + methodSymbol.InterceptionContextName() + " context)",
                    "       {",
                    "           throw new InvalidOperationException(\"" + contextNotCorrectTypeMessage + "\");",
                    "       }",
                    "",
                    "       var result = await " + baseMethodCall,
                    "",
                    "       return new " + methodSymbol.MethodResultName() + "(result);",
                    "   });",
                    "",
                    $"var ctx = new {methodSymbol.InterceptionContextName()}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))});",
                    "",
                    "var result = await pipeline.ExecuteAsync(ctx);",
                    "if(result is not " + methodSymbol.MethodResultName() + " methodResult)",
                    "{",
                    "    throw new InvalidOperationException(\"" + resultNotCorrectTypeMessage + "\");",
                    "}",
                    "",
                    "return methodResult.Result;"
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        classBodies.Add(newMethod);

        return
        [
            new CSharpRegion()
            {
                Name = $"{methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters)})",
                Body = classBodies
            }
        ];
    }

    private static CSharpClass GetContextClass(IMethodSymbol methodSymbol)
    {
        var contextClass = new CSharpClass()
        {
            Visibility = CSharpVisibility.Private,
            Name = methodSymbol.InterceptionContextName(),
            Inheritance = [$"{Constants.Namespace}.{Constants.ContextInterface}"],
            Type = CSharpClassType.Class,
            Body = []
        };

        var constructorBody = methodSymbol.Parameters
            .Select(p => $"{p.Name} = _{p.Name};")
            .ToList();

        constructorBody.AddRange([
            $"Method = new {Constants.Namespace}.{Constants.InterceptionMethodClass}()",
            "{",
            $"  MethodName = \"{methodSymbol.Name}\",",
            $"  ReturnType = \"{methodSymbol.ReturnType}\",",
            $"  Parameters = new List<{Constants.Namespace}.{Constants.InterceptionMethodParameterClass}>()",
            $"  {{",
            string.Join(",\n",
                methodSymbol.Parameters.Select(p =>
                    $"    new {Constants.Namespace}.{Constants.InterceptionMethodParameterClass}() {{ Name = \"{p.Name}\", Type = \"{p.Type}\" }}")),
            $"  }}",
            "};"
        ]);

        contextClass.Body.Add(new CSharpConstructor()
        {
            Visibility = CSharpVisibility.Public,
            Name = methodSymbol.InterceptionContextName(),
            Parameters = methodSymbol.Parameters
                .Select(p => new CSharpParameter()
                {
                    Type = p.Type.ToDisplayString(),
                    Name = $"_{p.Name}"
                })
                .ToList(),
            BodyLines = constructorBody
        });

        contextClass.Body.AddRange(methodSymbol.Parameters
            .Select(p => new CSharpProperty()
            {
                Visibility = CSharpVisibility.Public,
                Type = p.Type.ToDisplayString(),
                Name = p.Name
            }));

        contextClass.Body.Add(new CSharpProperty()
        {
            Visibility = CSharpVisibility.Public,
            Name = "Method",
            Type = $"{Constants.Namespace}.{Constants.InterceptionMethodClass}",
        });

        contextClass.Body.Add(new CSharpMethod()
        {
            Name = "GetParameter",
            ReturnType = "object",
            Visibility = CSharpVisibility.Public,
            Parameters =
            [
                new CSharpParameter()
                {
                    Name = "name",
                    Type = "string"
                }
            ],
            BodyLines = methodSymbol.Parameters
                .Select(p => $"if(name == \"{p.Name}\") return {p.Name};")
                .Concat(new List<string>()
                {
                    "throw new InvalidOperationException($\"Parameter with name {name} not found\");"
                })
                .ToList()
        });

        contextClass.Body.Add(new CSharpMethod()
        {
            Name = "SetParameter",
            ReturnType = "void",
            Visibility = CSharpVisibility.Public,
            Parameters =
            [
                new CSharpParameter()
                {
                    Name = "name",
                    Type = "string"
                },
                new CSharpParameter()
                {
                    Name = "value",
                    Type = "object"
                }
            ],
            BodyLines = methodSymbol.Parameters
                .Select(p =>
                    $"if(name == \"{p.Name}\" && value is {p.Type.ToDisplayString()} typedValue) \n {{\n\t{p.Name} = typedValue;\n\treturn;\n}}")
                .Concat(new List<string>()
                {
                    "throw new InvalidOperationException($\"Parameter with name {name} not found\");"
                })
                .ToList()
        });

        var methodType = methodSymbol.GetMethodType();
        var acceptsCreateResult = methodType == Extensions.MethodType.AsyncValue ||
                                  methodType == Extensions.MethodType.SyncValue;

        contextClass.Body.Add(new CSharpMethod()
        {
            Name = "CreateResult",
            ReturnType = $"{Constants.Namespace}.{Constants.MethodResultInterface}",
            Visibility = CSharpVisibility.Public,
            Parameters =
            [
                new CSharpParameter()
                {
                    Name = "value",
                    Type = "object?",
                    DefaultValue = "null"
                }
            ],
            BodyLines = acceptsCreateResult
                ?
                [
                    "if(value is not " + methodSymbol.UnwrapReturnTypeTask() + " resultAsTResult)",
                    "{",
                    "    throw new InvalidOperationException(\"Result is not of type " +
                    methodSymbol.UnwrapReturnTypeTask() + "\");",
                    "}",
                    "return new " + methodSymbol.MethodResultName() + "(resultAsTResult);"
                ]
                : ["throw new InvalidOperationException(\"Method does not return a value\");"]
        });

        return contextClass;
    }

    private static CSharpClass GetMethodResultClass(IMethodSymbol methodSymbol)
    {
        var body = methodSymbol.ReturnsVoid || methodSymbol.IsReturningEmptyTask()
            ? new List<CSharpClassBody>()
            :
            [
                new CSharpConstructor()
                {
                    Visibility = CSharpVisibility.Public,
                    Name = methodSymbol.MethodResultName(),
                    Parameters =
                    [
                        new CSharpParameter()
                        {
                            Type = methodSymbol.UnwrapReturnTypeTask(),
                            Name = "result"
                        }
                    ],
                    BodyLines = ["Result = result;"]
                },
                new CSharpProperty()
                {
                    Visibility = CSharpVisibility.Public,
                    Type = methodSymbol.UnwrapReturnTypeTask(),
                    Name = "Result",
                }
            ];

        var methodType = methodSymbol.GetMethodType();

        var acceptsChangingResult = methodType == Extensions.MethodType.AsyncValue ||
                                    methodType == Extensions.MethodType.SyncValue;

        body.Add(new CSharpMethod
        {
            Visibility = CSharpVisibility.Public,
            ReturnType = "object",
            IsOverride = false,
            IsAsync = false,
            Name = "GetResult",
            BodyLines = acceptsChangingResult
                ?
                [
                    "Result;"
                ]
                : ["throw new InvalidOperationException(\"Method does not return a value\");"]
        });

        body.Add(new CSharpMethod
        {
            Visibility = CSharpVisibility.Public,
            ReturnType = "void",
            IsOverride = false,
            IsAsync = false,
            Name = "SetResult",
            Parameters =
            [
                new CSharpParameter()
                {
                    Type = "object",
                    Name = acceptsChangingResult ? "result" : "_",
                }
            ],
            BodyLines = acceptsChangingResult
                ?
                [
                    "if(result is not " + methodSymbol.UnwrapReturnTypeTask() + " resultAsTResult)",
                    "{",
                    "    throw new InvalidOperationException(\"Result is not of type " +
                    methodSymbol.UnwrapReturnTypeTask() + "\");",
                    "}",
                    "Result = resultAsTResult;"
                ]
                : ["throw new InvalidOperationException(\"Method does not return a value\");"]
        });

        return new CSharpClass()
        {
            Visibility = CSharpVisibility.Private,
            Name = methodSymbol.MethodResultName(),
            Inheritance = [$"{Constants.Namespace}.{Constants.MethodResultInterface}"],
            Type = CSharpClassType.Class,
            Body = body
        };
    }
}