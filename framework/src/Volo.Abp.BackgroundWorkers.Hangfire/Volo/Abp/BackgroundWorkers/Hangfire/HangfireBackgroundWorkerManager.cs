using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Hangfire;

namespace Volo.Abp.BackgroundWorkers.Hangfire;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IBackgroundWorkerManager), typeof(HangfireBackgroundWorkerManager))]
public class HangfireBackgroundWorkerManager : BackgroundWorkerManager, ISingletonDependency
{
    protected AbpHangfireBackgroundJobServer BackgroundJobServer { get; set; } = default!;
    protected IServiceProvider ServiceProvider { get; }

    public HangfireBackgroundWorkerManager(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Initialize()
    {
        BackgroundJobServer = ServiceProvider.GetRequiredService<AbpHangfireBackgroundJobServer>();
    }

    public override async Task AddAsync(IBackgroundWorker worker, CancellationToken cancellationToken = default)
    {
        var abpHangfireOptions = ServiceProvider.GetRequiredService<IOptions<AbpHangfireOptions>>().Value;
        var defaultQueuePrefix = abpHangfireOptions.DefaultQueuePrefix;
        var defaultQueue = abpHangfireOptions.DefaultQueue;

        switch (worker)
        {
            case IHangfireBackgroundWorker hangfireBackgroundWorker:
            {
                var unProxyWorker = ProxyHelper.UnProxy(hangfireBackgroundWorker);

                RecurringJob.AddOrUpdate(
                    hangfireBackgroundWorker.RecurringJobId,
                    hangfireBackgroundWorker.Queue.IsNullOrWhiteSpace() ? defaultQueue : defaultQueuePrefix + hangfireBackgroundWorker.Queue,
                    () => ((IHangfireBackgroundWorker)unProxyWorker).DoWorkAsync(cancellationToken),
                    hangfireBackgroundWorker.CronExpression,
                    new RecurringJobOptions
                    {
                        TimeZone = hangfireBackgroundWorker.TimeZone
                    });

                break;
            }
            case AsyncPeriodicBackgroundWorkerBase or PeriodicBackgroundWorkerBase:
            {
                int? period = null;
                string? cronExpression = null;

                switch (worker)
                {
                    case AsyncPeriodicBackgroundWorkerBase asyncPeriodicBackgroundWorkerBase:
                        period = asyncPeriodicBackgroundWorkerBase.Period;
                        cronExpression = asyncPeriodicBackgroundWorkerBase.CronExpression;
                        break;
                    case PeriodicBackgroundWorkerBase periodicBackgroundWorkerBase:
                        period = periodicBackgroundWorkerBase.Period;
                        cronExpression = periodicBackgroundWorkerBase.CronExpression;
                        break;
                }

                if (period == null && cronExpression.IsNullOrWhiteSpace())
                {
                    var logger = ServiceProvider.GetRequiredService<ILogger<HangfireBackgroundWorkerManager>>();
                    logger.LogError(
                        $"Cannot add periodic background worker {worker.GetType().FullName} to Hangfire scheduler, because both Period and CronExpression are not set. " +
                        "You can either set Period or CronExpression property of the worker."
                    );
                    return;
                }

                var workerAdapter = (ServiceProvider.GetRequiredService(typeof(HangfirePeriodicBackgroundWorkerAdapter<>).MakeGenericType(ProxyHelper.GetUnProxiedType(worker))) as IHangfireBackgroundWorker)!;
                Expression<Func<Task>> methodCall = () => workerAdapter.DoWorkAsync(cancellationToken);
                var recurringJobId = !workerAdapter.RecurringJobId.IsNullOrWhiteSpace() ? workerAdapter.RecurringJobId : GetRecurringJobId(worker, methodCall);

                RecurringJob.AddOrUpdate(
                    recurringJobId,
                    workerAdapter.Queue.IsNullOrWhiteSpace() ? defaultQueue : defaultQueuePrefix + workerAdapter.Queue,
                    methodCall,
                    cronExpression ?? GetCron(period!.Value),
                    new RecurringJobOptions
                    {
                        TimeZone = workerAdapter.TimeZone
                    });
                break;
            }
            default:
                await base.AddAsync(worker, cancellationToken);
                break;
        }
    }

    private static readonly MethodInfo? GetRecurringJobIdMethodInfo = typeof(RecurringJob).GetMethod("GetRecurringJobId", BindingFlags.NonPublic | BindingFlags.Static);
    protected virtual string? GetRecurringJobId(IBackgroundWorker worker, Expression<Func<Task>> methodCall)
    {
        string? recurringJobId = null;
        if (GetRecurringJobIdMethodInfo != null)
        {
            var job = Job.FromExpression(methodCall);
            recurringJobId = (string)GetRecurringJobIdMethodInfo.Invoke(null, [job])!;
        }

        if (recurringJobId.IsNullOrWhiteSpace())
        {
            recurringJobId = $"HangfirePeriodicBackgroundWorkerAdapter<{worker.GetType().Name}>.DoWorkAsync";
        }

        return recurringJobId;
    }

    protected virtual string GetCron(int period)
    {
        var time = TimeSpan.FromMilliseconds(period);
        string cron;

        if (time.TotalSeconds <= 59)
        {
            cron = $"*/{time.TotalSeconds} * * * * *";
        }
        else if (time.TotalMinutes <= 59)
        {
            cron = $"*/{time.TotalMinutes} * * * *";
        }
        else if (time.TotalHours <= 23)
        {
            cron = $"0 */{time.TotalHours} * * *";
        }
        else if(time.TotalDays <= 31)
        {
            cron = $"0 0 0 1/{time.TotalDays} * *";
        }
        else
        {
            throw new AbpException($"Cannot convert period: {period} to cron expression, use HangfireBackgroundWorkerBase to define worker");
        }

        return cron;
    }
}
