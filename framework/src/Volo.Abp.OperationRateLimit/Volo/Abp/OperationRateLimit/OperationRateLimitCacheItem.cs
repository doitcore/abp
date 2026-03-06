using System;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.OperationRateLimit;

[CacheName("OperationRateLimit")]
[IgnoreMultiTenancy]
public class OperationRateLimitCacheItem
{
    public int Count { get; set; }

    public DateTimeOffset WindowStart { get; set; }
}
