namespace Interceptus.Generator;

internal static class Constants
{
    public const string Namespace = "Interceptus";
    public const string ClassMarkerAttribute = "InterceptingAttribute";
    public const string MethodMarkerAttribute = "InterceptorAttribute";
    public const string ContextInterface = "IInterceptionContext";
    public const string MethodResultInterface = "IMethodResult";
    public const string PipelineAsyncClass = "InterceptorPipelineAsync";
    public const string PipelineClass = "InterceptorPipeline";
    public const string InterceptionMethodClass = "InterceptionMethod";
    public const string InterceptionMethodParameterClass = "InterceptionMethodParameter";
    public const string InterceptorResolverInterface = "IInterceptorResolver";
}