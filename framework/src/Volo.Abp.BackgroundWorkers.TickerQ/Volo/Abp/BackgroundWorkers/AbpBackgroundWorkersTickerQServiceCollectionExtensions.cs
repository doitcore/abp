using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers.TickerQ;

namespace Volo.Abp.BackgroundWorkers;

/// <summary>
/// Extension methods for TickerQ background worker integration.
/// </summary>
public static class AbpBackgroundWorkersTickerQServiceCollectionExtensions
{
    /// <summary>
    /// Adds TickerQ background worker services to the service collection.
    /// This method provides additional configuration options beyond the basic module registration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAbpTickerQBackgroundWorkers(
        this IServiceCollection services,
        Action<AbpBackgroundWorkerTickerQOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }
}