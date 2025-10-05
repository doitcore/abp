# TickerQ Background Job Manager

[TickerQ](https://tickerq.net/) is a fast, reflection-free background task scheduler for .NET — built with source generators, EF Core integration, cron + time-based execution, and a real-time dashboard. You can integrate TickerQ with the ABP to use it instead of the [default background job manager](../background-jobs). In this way, you can use the same background job API for TickerQ and your code will be independent of TickerQ. If you like, you can directly use TickerQ's API, too.

> See the [background jobs document](../background-jobs) to learn how to use the background job system. This document only shows how to install and configure the TickerQ integration.

## Installation

It is suggested to use the [ABP CLI](../../../cli) to install this package.

### Using the ABP CLI

Open a command line window in the folder of the project (.csproj file) and type the following command:

````bash
abp add-package Volo.Abp.BackgroundJobs.TickerQ
````

>  If you haven't done it yet, you first need to install the [ABP CLI](../../../cli). For other installation options, see [the package description page](https://abp.io/package-detail/Volo.Abp.BackgroundJobs.TickerQ).

### Manual Installation

If you want to manually install;

1. Add the [Volo.Abp.BackgroundJobs.TickerQ](https://www.nuget.org/packages/Volo.Abp.BackgroundJobs.TickerQ) NuGet package to your project:

   ````
   dotnet add package Volo.Abp.BackgroundJobs.TickerQ
   ````

2. Add the `AbpBackgroundJobsTickerQModule` to the dependency list of your module:

````csharp
[DependsOn(
	//...other dependencies
	typeof(AbpBackgroundJobsTickerQModule) //Add the new module dependency
	)]
public class YourModule : AbpModule
{
}
````

## Configuration

### AbpBackgroundJobsTickerQOptions

You can configure the `TimeTicker` properties for specific jobs. For example, Change `Priority`, `Retries` and `RetryIntervals` properties:

```csharp
Configure<AbpBackgroundJobsTickerQOptions>(options =>
{
	options.AddJobConfiguration<MyJob>(new AbpBackgroundJobsTimeTickerConfiguration()
	{
		Retries = 3,
		RetryIntervals = new[] {30, 60, 120}, // Retry after 30s, 60s, then 2min
		Priority = TickerTaskPriority.High
	});

	options.AddJobConfiguration<MyJob2>(new AbpBackgroundJobsTimeTickerConfiguration()
	{
		Retries = 5,
		RetryIntervals = new[] {30, 60, 120}, // Retry after 30s, 60s, then 2min
		Priority = TickerTaskPriority.Normal
	});
});
```

### TickerQ Dashboard and EF Core Integration

You can install the [TickerQ dashboard](https://tickerq.net/setup/dashboard.html) and [Entity Framework Core](https://tickerq.net/setup/tickerq-ef-core.html) integration by its documentation. There is no specific configuration needed for the ABP integration.
