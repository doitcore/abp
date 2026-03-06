using System;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitRuleDefinition
{
    public TimeSpan Duration { get; set; }

    public int MaxCount { get; set; }

    public OperationRateLimitPartitionType PartitionType { get; set; }

    public Func<OperationRateLimitContext, string>? CustomPartitionKeyResolver { get; set; }

    public bool IsMultiTenant { get; set; }
}
