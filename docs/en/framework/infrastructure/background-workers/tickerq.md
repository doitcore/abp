# TickerQ Background Worker Manager

[TickerQ](https://github.com/dotnetdevelopersdz/TickerQ) is a fast, reflection-free background task scheduler for .NET — built with source generators, EF Core integration, cron + time-based execution, and a real-time dashboard. You can integrate TickerQ with the ABP Framework to use it instead of the [default background worker manager](../background-workers).

The major advantages of TickerQ include:
- **Performance**: Reflection-free design with source generators for optimal performance
- **EF Core Integration**: Native support for Entity Framework Core for job persistence
- **Flexible Scheduling**: Support for both cron expressions and time-based execution
- **Real-time Dashboard**: Built-in dashboard for monitoring and managing background jobs
- **Modern .NET**: Built for modern .NET with async/await support throughout

## Installation

It is suggested to use the [ABP CLI](../../../cli) to install this package.

### Using the ABP CLI

Open a command line window in the folder of the project (.csproj file) and type the following command:

````bash
abp add-package Volo.Abp.BackgroundWorkers.TickerQ
````

### Manual Installation

If you want to manually install:

1. Add the [Volo.Abp.BackgroundWorkers.TickerQ](https://www.nuget.org/packages/Volo.Abp.BackgroundWorkers.TickerQ) NuGet package to your project:

   ````
   dotnet add package Volo.Abp.BackgroundWorkers.TickerQ
   ````

2. Add the `AbpBackgroundWorkersTickerQModule` to the dependency list of your module:

````csharp
[DependsOn(
    //...other dependencies
    typeof(AbpBackgroundWorkersTickerQModule) //Add the new module dependency
    )]
public class YourModule : AbpModule
{
}
````

> TickerQ background worker integration provides an adapter `TickerQPeriodicBackgroundWorkerAdapter` to automatically load any `PeriodicBackgroundWorkerBase` and `AsyncPeriodicBackgroundWorkerBase` derived classes as `ITickerQBackgroundWorker` instances. This allows you to easily switch over to use TickerQ as the background manager even if you have existing background workers that are based on the [default background workers implementation](../background-workers).

## Configuration

You need to configure TickerQ with your preferred storage provider. TickerQ supports various storage options including Entity Framework Core.

1. First, configure TickerQ in your module's `ConfigureServices` method:

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var configuration = context.Services.GetConfiguration();
    var hostingEnvironment = context.Services.GetHostingEnvironment();

    //... other configurations.

    ConfigureTickerQ(context, configuration);
}

private void ConfigureTickerQ(ServiceConfigurationContext context, IConfiguration configuration)
{
    // TODO: Configure TickerQ here when the package becomes available
    // This would typically involve setting up the database connection,
    // configuring the scheduler options, and setting up the dashboard
}
````

2. You can configure the ABP TickerQ integration options:

````csharp
Configure<AbpBackgroundWorkerTickerQOptions>(options =>
{
    options.IsAutoRegisterEnabled = true; // Auto-register TickerQ workers
    options.DefaultCronExpression = "0 * * ? * *"; // Default: every minute
    options.DefaultMaxRetryAttempts = 3; // Default retry attempts
    options.DefaultPriority = 0; // Default priority
});
````

## Create a Background Worker

`TickerQBackgroundWorkerBase` is an easy way to create a background worker.

````csharp
public class MyLogWorker : TickerQBackgroundWorkerBase
{
    public MyLogWorker()
    {
        JobId = nameof(MyLogWorker);
        CronExpression = "0 */10 * ? * *"; // Every 10 minutes
        Priority = 1; // Higher priority
        MaxRetryAttempts = 5; // Retry up to 5 times on failure
    }

    public override Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Executed MyLogWorker with TickerQ!");
        return Task.CompletedTask;
    }
}
````

### Properties

* **JobId** - A unique identifier for the job (optional, defaults to the class name)
* **CronExpression** - A CRON expression for scheduling (see [CRON expression](https://en.wikipedia.org/wiki/Cron#CRON_expression))
* **Priority** - Job priority (higher values = higher priority, default is 0)
* **MaxRetryAttempts** - Maximum number of retry attempts on failure (default is 3)
* **AutoRegister** - Whether to automatically register this worker (default is true)

> You can directly implement the `ITickerQBackgroundWorker` interface, but `TickerQBackgroundWorkerBase` provides useful properties like Logger and service access.

## Register Background Workers

TickerQ background workers are automatically registered if `AutoRegister` is `true` (default). However, you can also manually register them:

````csharp
[DependsOn(typeof(AbpBackgroundWorkersTickerQModule))]
public class MyModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(
        ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<MyLogWorker>();
    }
}
````

## Migrating from Other Background Worker Implementations

TickerQ integration provides adapters for existing background workers:

### From Default Background Workers

Existing `AsyncPeriodicBackgroundWorkerBase` and `PeriodicBackgroundWorkerBase` workers will automatically work with TickerQ through the adapter system. The adapter will convert timer periods to appropriate cron expressions.

### From Quartz or Hangfire

When migrating from Quartz or Hangfire, you can:

1. Keep existing workers unchanged (they'll work through adapters)
2. Gradually migrate to `TickerQBackgroundWorkerBase` for better performance and features
3. Use the native TickerQ features like source generator optimizations

## Dashboard Integration

TickerQ provides a real-time dashboard for monitoring background jobs. To enable the dashboard:

````csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    
    // ... others
    
    // TODO: Add TickerQ dashboard integration when available
    // app.UseTickerQDashboard("/tickerq"); 
    
    app.UseConfiguredEndpoints();
}
````

## Advanced Features

### Source Generator Optimizations

TickerQ uses source generators to eliminate reflection and improve performance. When using TickerQ-specific features, your jobs will benefit from:

- Compile-time job registration
- Zero-allocation job execution
- Optimized serialization

### EF Core Integration

TickerQ provides native Entity Framework Core integration for job persistence:

````csharp
public class MyEfCoreWorker : TickerQBackgroundWorkerBase
{
    private readonly IRepository<MyEntity> _repository;

    public MyEfCoreWorker(IRepository<MyEntity> repository)
    {
        _repository = repository;
        JobId = nameof(MyEfCoreWorker);
        CronExpression = "0 0 2 ? * *"; // Daily at 2 AM
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Work with EF Core entities
        var entities = await _repository.GetListAsync(cancellationToken: cancellationToken);
        
        // Process entities...
        
        Logger.LogInformation("Processed {Count} entities", entities.Count);
    }
}
````

## See Also

* [Background Workers](../background-workers)
* [Background Jobs](../background-jobs)
* [Quartz Background Worker Manager](./quartz.md)
* [Hangfire Background Worker Manager](./hangfire.md)