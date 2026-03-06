using System;
using System.Threading.Tasks;

namespace Volo.Abp.OperationRateLimit;

public interface IOperationRateLimitStore
{
    Task<OperationRateLimitStoreResult> IncrementAsync(string key, TimeSpan duration, int maxCount);

    Task<OperationRateLimitStoreResult> GetAsync(string key, TimeSpan duration, int maxCount);

    Task ResetAsync(string key);
}
