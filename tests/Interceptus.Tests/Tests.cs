using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Interceptus.Tests;

public class Tests
{
    [Test]
    public void InterceptorSync()
    {
        // Arrange
        var mock = new Mock<ITestInterceptor>();
        
        var services = new ServiceCollection()
            .AddInterceptus()
            .AddSingleton<ITestInterceptor>(mock.Object)
            .AddSingleton<TestService, TestServiceProxy>()
            .BuildServiceProvider();
        
        var service = services.GetRequiredService<TestService>();
        
        // Act
        service.Test();
        
        // Assert
        mock.Verify(x => x.Invoke(It.IsAny<IInterceptionContext>(), It.IsAny<Func<IInterceptionContext, IMethodResult>>()), Times.Once);
    }
    
    [Test]
    public async Task InterceptorAsync()
    {
        // Arrange
        var mock = new Mock<ITestInterceptor>();
        
        var services = new ServiceCollection()
            .AddInterceptus()
            .AddSingleton<ITestInterceptor>(mock.Object)
            .AddSingleton<TestService, TestServiceProxy>()
            .BuildServiceProvider();
        
        var service = services.GetRequiredService<TestService>();
        
        // Act
        await service.TestAsync();
        
        // Assert
        mock.Verify(x => x.InvokeAsync(It.IsAny<IInterceptionContext>(), It.IsAny<Func<IInterceptionContext, Task<IMethodResult>>>()), Times.Once);
    }
}

public interface ITestInterceptor : IInterceptor
{
    
}

[Intercepting]
internal class TestService
{
    [Interceptor<ITestInterceptor>]
    public virtual void Test()
    {
        Console.WriteLine("Test");
    }
    
    [Interceptor<ITestInterceptor>]
    public virtual Task TestAsync()
    {
        Console.WriteLine("TestAsync");
        return Task.CompletedTask;
    }
}