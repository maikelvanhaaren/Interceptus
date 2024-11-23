# Interceptus

**Interceptus** is a .NET package which allows you to intercept method calls on classes through source-generated proxy classes.

## Installation 📦

To install the package, run:
```bash
dotnet add package Interceptus
```

## Usage 🛠️
Here's a quick example to get started:
```csharp
// Define a custom interceptor
public class ExampleInterceptor : IInterceptor
{
    public IMethodResult Invoke(IInterceptionContext context, Func<IInterceptionContext, IMethodResult> next)
    {
        // Modify parameters before proceeding
        if(context.GetParameter("value") is int value)
        {
            context.SetParameter("value", value + 1);
        }
                    
        return next(context);
    }

    public async Task<IMethodResult> InvokeAsync(IInterceptionContext context, Func<IInterceptionContext, Task<IMethodResult>> next)
    {
        // Handle asynchronous method calls
        (...)
    }
}

[Intercepting]
public class Service
{
    [Interceptor<ExampleInterceptor>]
    public virtual int DoSomething(int value) => value;
}


var services = new ServiceCollection()
    .AddInterceptus()
    .AddSingleton<ExampleInterceptor>()
    .AddSingleton<Service, ServiceProxy>() // Register the proxy class instead of the original class
    .BuildServiceProvider();

var service = services.GetRequiredService<Service>();

var value = service.DoSomething(value: 100);

Console.WriteLine(value); // Output: 101
Console.WriteLine(service.GetType().Name); // Output: ServiceProxy
```

## Roadmap 🗺️

We welcome suggestions and feedback! Feel free to create issues for new feature requests. Below are some planned features, depending on community interest:

- [x] Basic implementation for intercepting method calls.
- [ ] Improve test coverage.
- [ ] Support for intercepting properties.
- [ ] Support for intercepting constructors.
- [ ] Support for intercepting in classes which are defined in other classes.
- [ ] Probably better documentation for usages.

## Contributing 🤝
Contributions are always welcome! If you plan to make significant changes, please open an issue first to discuss your ideas.

## License 📜
This project is licensed under the [MIT](https://choosealicense.com/licenses/mit/)

