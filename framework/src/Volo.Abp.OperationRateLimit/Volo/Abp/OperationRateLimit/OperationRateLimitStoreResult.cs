using System;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitStoreResult
{
    public bool IsAllowed { get; set; }

    public int CurrentCount { get; set; }

    public int MaxCount { get; set; }

    public TimeSpan? RetryAfter { get; set; }
}
