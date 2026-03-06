using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.OperationRateLimit;

public class DefaultOperationRateLimitPolicyProvider : IOperationRateLimitPolicyProvider, ITransientDependency
{
    protected AbpOperationRateLimitOptions Options { get; }

    public DefaultOperationRateLimitPolicyProvider(IOptions<AbpOperationRateLimitOptions> options)
    {
        Options = options.Value;
    }

    public virtual Task<OperationRateLimitPolicy> GetAsync(string policyName)
    {
        if (!Options.Policies.TryGetValue(policyName, out var policy))
        {
            throw new AbpException(
                $"Operation rate limit policy '{policyName}' was not found. " +
                $"Make sure to configure it using AbpOperationRateLimitOptions.AddPolicy().");
        }

        return Task.FromResult(policy);
    }

    public virtual Task<List<OperationRateLimitPolicy>> GetListAsync()
    {
        return Task.FromResult(Options.Policies.Values.ToList());
    }
}
