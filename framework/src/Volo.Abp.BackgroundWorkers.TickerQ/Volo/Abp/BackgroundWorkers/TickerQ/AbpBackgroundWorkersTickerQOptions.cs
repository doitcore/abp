using System;
using System.Collections.Generic;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

public class AbpBackgroundWorkersTickerQOptions
{
    private readonly Dictionary<Type, AbpBackgroundWorkersCronTickerConfiguration> _onfigurations;

    public AbpBackgroundWorkersTickerQOptions()
    {
        _onfigurations = new Dictionary<Type, AbpBackgroundWorkersCronTickerConfiguration>();
    }

    public void AddConfiguration<TJob>(AbpBackgroundWorkersCronTickerConfiguration configuration)
    {
        AddConfiguration(typeof(TJob), configuration);
    }

    public void AddConfiguration(Type jobType, AbpBackgroundWorkersCronTickerConfiguration configuration)
    {
        _onfigurations[jobType] = configuration;
    }

    public AbpBackgroundWorkersCronTickerConfiguration? GetConfigurationOrNull<TJob>()
    {
        return GetConfigurationOrNull(typeof(TJob));
    }

    public AbpBackgroundWorkersCronTickerConfiguration? GetConfigurationOrNull(Type jobType)
    {
        return _onfigurations.GetValueOrDefault(jobType);
    }
}
