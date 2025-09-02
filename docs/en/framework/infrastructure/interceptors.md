# Interceptors

ABP provides a powerful interception system that allows you to execute custom logic before and after method calls without modifying the original method code. This is achieved through **dynamic proxying** and is extensively used throughout the ABP framework to implement cross-cutting concerns. ABP's interception is implemented on top of the Castle DynamicProxy library.

## What is Dynamic Proxying / Interception?

**Interception** is a technique that allows executing additional logic before or after a method call without directly modifying the method's code. This is achieved through **dynamic proxying**, where the runtime generates proxy classes that wrap the original class.

When a method is called on a proxied object:
1. The call is intercepted by the proxy
2. Custom behaviors (like logging, validation, or authorization) can be executed
3. The original method is called
4. Additional logic can be executed after the method completes

This enables **cross-cutting concerns** (logic that applies across many parts of an application) to be handled in a clean, reusable way without code duplication.

## Similarities and Differences with MVC Action/Page Filters

If you are familiar with ASP.NET Core MVC, you've likely used **action filters** or **page filters**. Interceptors are conceptually similar but have some key differences:

### Similarities

* Both allow executing code before and after method execution
* Both are used to implement cross-cutting concerns like validation, logging, caching, or exception handling
* Both support asynchronous operations

### Differences

* **Scope**: Filters are tied to MVC's request pipeline, while interceptors can be applied to **any class or service** in the application
* **Configuration**: Filters are configured via attributes or middleware in MVC, while interceptors in ABP are applied through **dependency injection and dynamic proxies**
* **Target**: Interceptors can target application services, domain services, repositories, and virtually any service resolved from the IoC container—not just web controllers

## How ABP Uses Interceptors

ABP Framework extensively leverages interception to provide built-in features without requiring boilerplate code. Here are some key examples:

### Unit of Work (UOW)

Automatically begins and commits/rolls back a database transaction when entering or exiting an application service method. This ensures data consistency without manual transaction management.

### Input Validation

Input DTOs are automatically validated against data annotation attributes and custom validation rules before executing the service logic, providing consistent validation behavior across all services.

### Authorization

Checks user permissions before allowing the execution of application service methods, ensuring security policies are enforced consistently.

### Feature & Global Feature Checking

Checks if a feature is enabled before executing the service logic, allowing you to conditionally enable or restrict functionality for tenants or the application.

### Auditing

Automatically logs who performed an action, when it happened, what parameters were used, and what data was involved, providing comprehensive audit trails.

## Building Your Own Interceptor

You can create custom interceptors in ABP to implement your own cross-cutting concerns.

### Creating an Interceptor

Create a class that inherits from `AbpInterceptor`:

````csharp
using System.Threading.Tasks;
using Volo.Abp.Aspects;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

public class ExecutionTimeLogInterceptor : AbpInterceptor, ITransientDependency
{
    private readonly ILogger<ExecutionTimeLogInterceptor> _logger;

    public ExecutionTimeLogInterceptor(ILogger<ExecutionTimeLogInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task InterceptAsync(IAbpMethodInvocation invocation)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Executing {invocation.TargetObject.GetType().Name}.{invocation.Method.Name}");

        // Proceed to the actual target method
        await invocation.ProceedAsync();

        sw.Stop();

        _logger.LogInformation("Executed {invocation.TargetObject.GetType().Name}.{invocation.Method.Name} in {sw.ElapsedMilliseconds} ms");
    }
}
````

### Register Interceptors

Create a static class that contains the `RegisterIfNeeded` method and register the interceptor in the `PreConfigureServices` method of your module.

The `ShouldIntercept` method is used to determine if the interceptor should be registered for the given type. You can add an `IExecutionTimeLogEnabled` interface and implement it in the classes that you want to intercept.

> `DynamicProxyIgnoreTypes` is static class that contains the types that should be ignored by the interceptor. See [Performance Considerations](#performance-considerations) for more information.

````csharp
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

public static class ExecutionTimeLogInterceptorRegistrar
{
    public static void RegisterIfNeeded(IOnServiceRegistredContext context)
    {
        if (ShouldIntercept(context.ImplementationType))
        {
            context.Interceptors.TryAdd<GlobalFeatureInterceptor>();
        }
    }

    private static bool ShouldIntercept(Type type)
    {
        return !DynamicProxyIgnoreTypes.Contains(type) && typeof(IExecutionTimeLogEnabled).IsAssignableFrom(type);
    }
}
````

````csharp
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    context.Services.OnRegistered(ExecutionTimeLogInterceptor.RegisterIfNeeded);
}
````

## Restrictions and Important Notes

### Virtual Methods Requirement

For **class proxies**, methods need to be marked as `virtual` so that they can be overridden by the proxy. Otherwise, interception will not occur.

````csharp
public class MyService : ITransientDependency
{
    // This method CANNOT be intercepted (not virtual)
    public void CannotBeIntercepted()
    {
    }

    // This method CAN be intercepted (virtual)
    public virtual void CanBeIntercepted()
    {
    }
}
````

> This restriction does **not** apply to interface-based proxies. If your service implements an interface and is injected via that interface, all methods can be intercepted regardless of the `virtual` keyword.

### Dependency Injection Scope

Interceptors only work when services are resolved from the dependency injection container. Direct instantiation with `new` bypasses interception:

````csharp
// This will NOT be intercepted
var service = new MyService();
service.CannotBeIntercepted();

// This WILL be intercepted (if MyService is registered with DI)
var service = serviceProvider.GetService<MyService>();
service.CanBeIntercepted();
````

### Performance Considerations

Interceptors are generally efficient, but each one adds method-call overhead. Keep the number of interceptors minimal on hot paths.

Castle DynamicProxy can negatively impact performance for certain components, notably ASP.NET Core MVC controllers. See the discussions in [castleproject/Core#486](https://github.com/castleproject/Core/issues/486) and [abpframework/abp#3180](https://github.com/abpframework/abp/issues/3180).

ABP uses interceptors for features like UOW, auditing, and authorization, which rely on dynamic proxy classes. For controllers, prefer implementing cross-cutting concerns with middleware or MVC/Page filters instead of dynamic proxies.

To avoid generating dynamic proxies for specific types, use the static class `DynamicProxyIgnoreTypes` and add the base classes of the types to the list. Subclasses of any listed base class are also ignored. ABP framework already adds some base classes to the list (`ComponentBase, ControllerBase, PageModel, ViewComponent`); you can add more base classes if needed.

> Always use interface-based proxies instead of class-based proxies for better performance.

## See Also

* [Castle DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md)
* [Castle.Core.AsyncInterceptor](https://github.com/JSkimming/Castle.Core.AsyncInterceptors)
* [ASP.NET Core Filters](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
* [ABP Video Introduction](https://abp.io/video-courses/essentials/interception)
