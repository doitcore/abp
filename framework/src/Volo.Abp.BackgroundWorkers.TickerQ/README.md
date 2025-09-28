# ABP TickerQ Background Workers Integration

This package provides integration between [TickerQ](https://github.com/dotnetdevelopersdz/TickerQ) and the ABP Framework's background worker system.

## About TickerQ

TickerQ is a fast, reflection-free background task scheduler for .NET — built with source generators, EF Core integration, cron + time-based execution, and a real-time dashboard.

Key features:
- **Performance**: Reflection-free design with source generators
- **EF Core Integration**: Native Entity Framework Core support
- **Flexible Scheduling**: Cron expressions and time-based execution
- **Real-time Dashboard**: Built-in monitoring and management
- **Modern .NET**: Built for modern .NET with async/await

## Installation

Install the NuGet package:

```bash
dotnet add package Volo.Abp.BackgroundWorkers.TickerQ
```

Or using the ABP CLI:

```bash
abp add-package Volo.Abp.BackgroundWorkers.TickerQ
```

## Usage

1. Add the module dependency to your ABP module:

```csharp
[DependsOn(typeof(AbpBackgroundWorkersTickerQModule))]
public class YourModule : AbpModule
{
    // ...
}
```

2. Create your background worker:

```csharp
public class MyTickerQWorker : TickerQBackgroundWorkerBase
{
    public MyTickerQWorker()
    {
        JobId = nameof(MyTickerQWorker);
        CronExpression = "0 */5 * ? * *"; // Every 5 minutes
        Priority = 1;
        MaxRetryAttempts = 3;
    }

    public override Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("TickerQ worker executed!");
        // Your work logic here
        return Task.CompletedTask;
    }
}
```

3. Configure options (optional):

```csharp
Configure<AbpBackgroundWorkerTickerQOptions>(options =>
{
    options.IsAutoRegisterEnabled = true;
    options.DefaultCronExpression = "0 * * ? * *";
    options.DefaultMaxRetryAttempts = 3;
    options.DefaultPriority = 0;
});
```

## Migration from Other Background Workers

The integration provides adapters for existing background workers:

- `AsyncPeriodicBackgroundWorkerBase` workers will work automatically
- `PeriodicBackgroundWorkerBase` workers will work automatically
- Timer periods are converted to appropriate cron expressions

## Features

- **Automatic Registration**: Workers are auto-registered by default
- **Dependency Injection**: Full DI support in workers
- **Error Handling**: Built-in retry logic and error handling
- **Performance**: Benefits from TickerQ's reflection-free design
- **Compatibility**: Works with existing ABP background workers

## Samples

See the `Samples` folder for example implementations demonstrating:
- Basic worker usage
- Error handling and retries
- Dependency injection
- Configuration options

## Documentation

For detailed documentation, see: [docs/en/framework/infrastructure/background-workers/tickerq.md](../../../docs/en/framework/infrastructure/background-workers/tickerq.md)

## Requirements

- .NET 8.0 or later
- ABP Framework 9.0 or later
- TickerQ package (when available)

## Status

This integration is ready for use once the TickerQ package becomes available on NuGet. The implementation follows ABP's established patterns for background worker integrations (Quartz, Hangfire) and provides a seamless migration path.