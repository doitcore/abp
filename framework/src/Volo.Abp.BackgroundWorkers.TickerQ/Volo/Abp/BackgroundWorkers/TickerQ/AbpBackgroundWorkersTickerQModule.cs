using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Modularity;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// ABP module for TickerQ background workers integration.
/// TickerQ is a fast, reflection-free background task scheduler for .NET — built with source generators, 
/// EF Core integration, cron + time-based execution, and a real-time dashboard.
/// </summary>
[DependsOn(
    typeof(AbpBackgroundWorkersModule))]
public class AbpBackgroundWorkersTickerQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register TickerQ-specific services
        // The TickerQBackgroundWorkerManager will automatically replace the default manager
        // due to the [Dependency(ReplaceServices = true)] attribute
    }
    
    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        // Check if background workers are enabled
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpBackgroundWorkerOptions>>().Value;
        if (!options.IsEnabled)
        {
            // If background workers are disabled, we don't need to initialize TickerQ
            return;
        }
        
        // Initialize TickerQ background worker manager
        var tickerQManager = context.ServiceProvider.GetRequiredService<TickerQBackgroundWorkerManager>();
        tickerQManager.Initialize(); 
    }
}