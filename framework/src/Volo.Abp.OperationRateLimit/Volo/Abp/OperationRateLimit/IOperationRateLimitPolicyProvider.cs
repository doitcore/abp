using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.OperationRateLimit;

public interface IOperationRateLimitPolicyProvider
{
    Task<OperationRateLimitPolicy> GetAsync(string policyName);

    Task<List<OperationRateLimitPolicy>> GetListAsync();
}
