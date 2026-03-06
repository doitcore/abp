using System;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitRuleResult
{
    public string RuleName { get; set; } = default!;

    public bool IsAllowed { get; set; }

    public int RemainingCount { get; set; }

    public int MaxCount { get; set; }

    public TimeSpan? RetryAfter { get; set; }

    public TimeSpan WindowDuration { get; set; }
}
