using System;
using System.Collections.Generic;

namespace Volo.Abp.OperationRateLimiting;

public class AbpOperationRateLimitingOptions
{
    public bool IsEnabled { get; set; } = true;

    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public Dictionary<string, OperationRateLimitingPolicy> Policies { get; } = new();

    public AbpOperationRateLimitingOptions AddPolicy(string name, Action<OperationRateLimitingPolicyBuilder> configure)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.NotNull(configure, nameof(configure));

        var builder = new OperationRateLimitingPolicyBuilder(name);
        configure(builder);
        Policies[name] = builder.Build();
        return this;
    }

    /// <summary>
    /// Configures an existing rate limiting policy by name.
    /// The builder is pre-populated with the existing policy's rules and error code,
    /// so you can add, clear, or replace rules while keeping what you don't change.
    /// Throws <see cref="AbpException"/> if the policy is not found.
    /// </summary>
    public AbpOperationRateLimitingOptions ConfigurePolicy(string name, Action<OperationRateLimitingPolicyBuilder> configure)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.NotNull(configure, nameof(configure));

        if (!Policies.TryGetValue(name, out var existingPolicy))
        {
            throw new AbpException(
                $"Could not find operation rate limiting policy: '{name}'. " +
                "Make sure the policy is defined with AddPolicy() before calling ConfigurePolicy().");
        }

        var builder = OperationRateLimitingPolicyBuilder.FromPolicy(existingPolicy);
        configure(builder);
        Policies[name] = builder.Build();
        return this;
    }
}
