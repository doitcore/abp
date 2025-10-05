using System;
using System.Threading;
using System.Threading.Tasks;
using TickerQ.Utilities.Enums;
using Volo.Abp.DependencyInjection;
using Volo.Abp.TickerQ;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IBackgroundWorkerManager), typeof(TickerQBackgroundWorkerManager))]
public class TickerQBackgroundWorkerManager : BackgroundWorkerManager, ISingletonDependency
{
    protected AbpTickerQFunctionProvider AbpTickerQFunctionProvider { get; }

    public TickerQBackgroundWorkerManager(AbpTickerQFunctionProvider abpTickerQFunctionProvider)
    {
        AbpTickerQFunctionProvider = abpTickerQFunctionProvider;
    }

    public override async Task AddAsync(IBackgroundWorker worker, CancellationToken cancellationToken = default)
    {
        if (worker is AsyncPeriodicBackgroundWorkerBase or PeriodicBackgroundWorkerBase)
        {
            int? period = null;
            string? cronExpression = null;

            if (worker is AsyncPeriodicBackgroundWorkerBase asyncPeriodicBackgroundWorkerBase)
            {
                period = asyncPeriodicBackgroundWorkerBase.Period;
                cronExpression = asyncPeriodicBackgroundWorkerBase.CronExpression;
            }
            else if (worker is PeriodicBackgroundWorkerBase periodicBackgroundWorkerBase)
            {
                period = periodicBackgroundWorkerBase.Period;
                cronExpression = periodicBackgroundWorkerBase.CronExpression;
            }

            if (period == null && cronExpression.IsNullOrWhiteSpace())
            {
                throw new AbpException($"Both 'Period' and 'CronExpression' are not set for {worker.GetType().FullName}. You must set at least one of them.");
            }

            if (period != null && cronExpression.IsNullOrWhiteSpace())
            {
                cronExpression = GetCron(period.Value);
            }

            var name = BackgroundWorkerNameAttribute.GetNameOrNull(worker.GetType()) ?? worker.GetType().FullName;
            AbpTickerQFunctionProvider.Functions.TryAdd(name!, (cronExpression!, TickerTaskPriority.LongRunning, async (tickerQCancellationToken, serviceProvider, tickerFunctionContext) =>
            {
                var workerInvoker = new TickerQPeriodicBackgroundWorkerInvoker(worker, serviceProvider);
                await workerInvoker.DoWorkAsync(tickerFunctionContext, tickerQCancellationToken);
            }));
        }

        await base.AddAsync(worker, cancellationToken);
    }

    protected virtual string GetCron(int period)
    {
        var time = TimeSpan.FromMilliseconds(period);
        if (time.TotalMinutes < 1)
        {
            // Less than 1 minute — 5-field cron doesn't support seconds, so run every minute
            return "* * * * *";
        }

        if (time.TotalMinutes < 60)
        {
            // Run every N minutes
            var minutes = (int)Math.Round(time.TotalMinutes);
            return $"*/{minutes} * * * *";
        }

        if (time.TotalHours < 24)
        {
            // Run every N hours
            var hours = (int)Math.Round(time.TotalHours);
            return $"0 */{hours} * * *";
        }

        if (time.TotalDays <= 31)
        {
            // Run every N days
            var days = (int)Math.Round(time.TotalDays);
            return $"0 0 */{days} * *";
        }

        throw new AbpException($"Cannot convert period: {period} to cron expression.");
    }
}
