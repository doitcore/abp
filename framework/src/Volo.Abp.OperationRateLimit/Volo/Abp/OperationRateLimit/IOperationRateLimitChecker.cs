using System.Threading.Tasks;

namespace Volo.Abp.OperationRateLimit;

public interface IOperationRateLimitChecker
{
    Task CheckAsync(string policyName, OperationRateLimitContext? context = null);

    Task<bool> IsAllowedAsync(string policyName, OperationRateLimitContext? context = null);

    Task<OperationRateLimitResult> GetStatusAsync(string policyName, OperationRateLimitContext? context = null);

    Task ResetAsync(string policyName, OperationRateLimitContext? context = null);
}
