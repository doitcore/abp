using System;
using System.Collections.Generic;

namespace Volo.Abp.OperationRateLimit;

public class OperationRateLimitPolicy
{
    public string Name { get; set; } = default!;

    public string? ErrorCode { get; set; }

    public List<OperationRateLimitRuleDefinition> Rules { get; set; } = new();

    public List<Type> CustomRuleTypes { get; set; } = new();
}
