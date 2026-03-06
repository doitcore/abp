using System;

namespace Volo.Abp.OperationRateLimit;

public interface IOperationRateLimitFormatter
{
    string Format(TimeSpan duration);
}
