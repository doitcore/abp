namespace Volo.Abp.BackgroundJobs.TickerQ;

public class AbpBackgroundJobsTimeTickerConfiguration
{
    public int? Retries { get; set; }

    public int[]? RetryIntervals { get; set; }
}
