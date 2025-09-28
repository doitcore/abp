using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers.TickerQ.Samples;
using Volo.Abp.Modularity;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// Sample module demonstrating how to use TickerQ background workers in an ABP application.
/// </summary>
[DependsOn(typeof(AbpBackgroundWorkersTickerQModule))]
public class SampleTickerQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure TickerQ options
        Configure<AbpBackgroundWorkerTickerQOptions>(options =>
        {
            options.IsAutoRegisterEnabled = true;
            options.DefaultCronExpression = "0 * * ? * *"; // Every minute
            options.DefaultMaxRetryAttempts = 3;
            options.DefaultPriority = 0;
        });

        // Register sample workers as transient services so they can be injected with dependencies
        context.Services.AddTransient<SampleTickerQWorker>();
        context.Services.AddTransient<SampleErrorHandlingTickerQWorker>();
        context.Services.AddTransient<SampleDependencyInjectionTickerQWorker>();
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        // Sample workers with AutoRegister = true will be registered automatically
        // But you can also register them manually if needed:
        
        // await context.AddBackgroundWorkerAsync<SampleTickerQWorker>();
        // await context.AddBackgroundWorkerAsync<SampleErrorHandlingTickerQWorker>();
        // await context.AddBackgroundWorkerAsync<SampleDependencyInjectionTickerQWorker>();
    }
}