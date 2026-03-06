using System.Threading.Tasks;

namespace Volo.Abp.OperationRateLimit;

public interface IOperationRateLimitRule
{
    Task<OperationRateLimitRuleResult> AcquireAsync(OperationRateLimitContext context);

    Task<OperationRateLimitRuleResult> CheckAsync(OperationRateLimitContext context);

    Task ResetAsync(OperationRateLimitContext context);
}
