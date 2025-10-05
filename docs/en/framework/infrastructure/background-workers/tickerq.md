# TickerQ Background Worker Manager

[TickerQ](https://tickerq.net/) is a fast, reflection-free background task scheduler for .NET — built with source generators, EF Core integration, cron + time-based execution, and a real-time dashboard. You can integrate TickerQ with the ABP to use it instead of the [default background worker manager](../background-workers).

## Installation

It is suggested to use the [ABP CLI](../../../cli) to install this package.

### Using the ABP CLI

Open a command line window in the folder of the project (.csproj file) and type the following command:

````bash
abp add-package Volo.Abp.BackgroundWorkers.TickerQ
````

### Manual Installation

If you want to manually install;

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

> TickerQ background worker integration provides an adapter `TickerQPeriodicBackgroundWorkerAdapter` to automatically load any `PeriodicBackgroundWorkerBase` and `AsyncPeriodicBackgroundWorkerBase` derived classes as `ITickerQBackgroundWorker` instances. This allows you to still to easily switch over to use TickerQ as the background manager even you have existing background workers that are based on the [default background workers implementation](../background-workers).

## Configuration

TODO: