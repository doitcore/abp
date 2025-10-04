using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.BackgroundJobs.TickerQ;

[Dependency(ReplaceServices = true)]
public class TickerQBackgroundJobManager : IBackgroundJobManager, ITransientDependency
{
    protected ITimeTickerManager<TimeTicker> TimeTickerManager { get; }
    protected AbpBackgroundJobOptions Options { get; }

    public TickerQBackgroundJobManager(ITimeTickerManager<TimeTicker> timeTickerManager, IOptions<AbpBackgroundJobOptions> options)
    {
        TimeTickerManager = timeTickerManager;
        Options = options.Value;
    }

    public virtual async Task<string> EnqueueAsync<TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
    {
        var result = await TimeTickerManager.AddAsync(new TimeTicker
        {
            Id = Guid.NewGuid(),
            Function = Options.GetJob(typeof(TArgs)).JobName,
            ExecutionTime = delay == null ? DateTime.UtcNow : DateTime.UtcNow.Add(delay.Value),
            Request = TickerHelper.CreateTickerRequest<TArgs>(args),

            //TODO: Make these configurable
            Retries = 3,
            RetryIntervals = [30, 60, 120], // Retry after 30s, 60s, then 2min
        });

        return result.Result.Id.ToString();
    }
}
