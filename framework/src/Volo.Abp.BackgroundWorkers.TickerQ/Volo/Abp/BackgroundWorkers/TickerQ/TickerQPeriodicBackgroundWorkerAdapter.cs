using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

/// <summary>
/// Adapter to enable existing periodic background workers to work with TickerQ.
/// This allows users to migrate from the default background worker implementation to TickerQ
/// without changing their existing worker code.
/// </summary>
public class TickerQPeriodicBackgroundWorkerAdapter<TWorker> : TickerQBackgroundWorkerBase, ITransientDependency
    where TWorker : class, IBackgroundWorker
{
    private readonly IServiceProvider _serviceProvider;

    public TickerQPeriodicBackgroundWorkerAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Set default job ID based on the worker type
        JobId = typeof(TWorker).Name;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Executing adapted periodic worker: {WorkerType}", typeof(TWorker).Name);
        
        using var scope = _serviceProvider.CreateScope();
        var worker = scope.ServiceProvider.GetRequiredService<TWorker>();
        
        try
        {
            if (worker is IPeriodicBackgroundWorker periodicWorker)
            {
                await periodicWorker.DoWorkAsync(cancellationToken);
            }
            else if (worker is AsyncPeriodicBackgroundWorkerBase asyncPeriodicWorker)
            {
                await asyncPeriodicWorker.DoWorkAsync(cancellationToken);
            }
            else if (worker is PeriodicBackgroundWorkerBase syncPeriodicWorker)
            {
                syncPeriodicWorker.DoWork();
                await Task.CompletedTask;
            }
            else
            {
                Logger.LogWarning("Worker {WorkerType} is not a supported periodic worker type", typeof(TWorker).Name);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing adapted periodic worker: {WorkerType}", typeof(TWorker).Name);
            throw;
        }
    }

    /// <summary>
    /// Configures the adapter based on the original worker's settings.
    /// </summary>
    public virtual TickerQPeriodicBackgroundWorkerAdapter<TWorker> Configure(TWorker originalWorker)
    {
        // Try to extract timing information from the original worker
        if (originalWorker is AsyncPeriodicBackgroundWorkerBase asyncWorker)
        {
            // Convert timer period to cron expression (approximate)
            if (asyncWorker.Timer?.Period != null)
            {
                var periodMinutes = asyncWorker.Timer.Period / 60000; // Convert ms to minutes
                if (periodMinutes < 1)
                {
                    CronExpression = "*/30 * * ? * *"; // Every 30 seconds for very short periods
                }
                else if (periodMinutes == 1)
                {
                    CronExpression = "0 * * ? * *"; // Every minute
                }
                else if (periodMinutes < 60)
                {
                    CronExpression = $"0 */{periodMinutes} * ? * *"; // Every N minutes
                }
                else
                {
                    var hours = periodMinutes / 60;
                    CronExpression = $"0 0 */{hours} ? * *"; // Every N hours
                }
            }
            
            // Use cron expression if available
            if (!string.IsNullOrEmpty(asyncWorker.CronExpression))
            {
                CronExpression = asyncWorker.CronExpression;
            }
        }
        else if (originalWorker is PeriodicBackgroundWorkerBase syncWorker)
        {
            // Similar logic for sync workers
            if (syncWorker.Timer?.Period != null)
            {
                var periodMinutes = syncWorker.Timer.Period / 60000;
                if (periodMinutes < 1)
                {
                    CronExpression = "*/30 * * ? * *";
                }
                else if (periodMinutes == 1)
                {
                    CronExpression = "0 * * ? * *";
                }
                else if (periodMinutes < 60)
                {
                    CronExpression = $"0 */{periodMinutes} * ? * *";
                }
                else
                {
                    var hours = periodMinutes / 60;
                    CronExpression = $"0 0 */{hours} ? * *";
                }
            }
        }

        return this;
    }
}