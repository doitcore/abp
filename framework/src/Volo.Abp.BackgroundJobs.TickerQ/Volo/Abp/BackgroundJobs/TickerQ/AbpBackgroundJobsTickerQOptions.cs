using System;
using System.Collections.Generic;

namespace Volo.Abp.BackgroundJobs.TickerQ;

public class AbpBackgroundJobsTickerQOptions
{
    private readonly Dictionary<Type, AbpBackgroundJobsTimeTickerConfiguration> _jobConfigurations;

    public AbpBackgroundJobsTickerQOptions()
    {
        _jobConfigurations = new Dictionary<Type, AbpBackgroundJobsTimeTickerConfiguration>();
    }

    public void AddJobConfiguration<TJob>(AbpBackgroundJobsTimeTickerConfiguration configuration)
    {
        AddJobConfiguration(typeof(TJob), configuration);
    }

    public void AddJobConfiguration(Type jobType, AbpBackgroundJobsTimeTickerConfiguration configuration)
    {
        _jobConfigurations[jobType] = configuration;
    }

    public AbpBackgroundJobsTimeTickerConfiguration? GetJobConfigurationOrNull<TJob>()
    {
        return GetJobConfigurationOrNull(typeof(TJob));
    }

    public AbpBackgroundJobsTimeTickerConfiguration? GetJobConfigurationOrNull(Type jobType)
    {
        return _jobConfigurations.GetValueOrDefault(jobType);
    }
}
