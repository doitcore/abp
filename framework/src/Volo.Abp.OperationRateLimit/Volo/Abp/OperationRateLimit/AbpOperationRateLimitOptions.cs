using System;
using System.Collections.Generic;

namespace Volo.Abp.OperationRateLimit;

public class AbpOperationRateLimitOptions
{
    public bool IsEnabled { get; set; } = true;

    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public Dictionary<string, OperationRateLimitPolicy> Policies { get; } = new();

    public void AddPolicy(string name, Action<OperationRateLimitPolicyBuilder> configure)
    {
        var builder = new OperationRateLimitPolicyBuilder(name);
        configure(builder);
        Policies[name] = builder.Build();
    }
}
