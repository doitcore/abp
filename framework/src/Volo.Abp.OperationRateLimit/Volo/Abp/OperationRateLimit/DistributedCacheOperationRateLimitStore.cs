using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Timing;

namespace Volo.Abp.OperationRateLimit;

public class DistributedCacheOperationRateLimitStore : IOperationRateLimitStore, ITransientDependency
{
    protected IDistributedCache<OperationRateLimitCacheItem> Cache { get; }
    protected IClock Clock { get; }
    protected IAbpDistributedLock DistributedLock { get; }
    protected AbpOperationRateLimitOptions Options { get; }

    public DistributedCacheOperationRateLimitStore(
        IDistributedCache<OperationRateLimitCacheItem> cache,
        IClock clock,
        IAbpDistributedLock distributedLock,
        IOptions<AbpOperationRateLimitOptions> options)
    {
        Cache = cache;
        Clock = clock;
        DistributedLock = distributedLock;
        Options = options.Value;
    }

    public virtual async Task<OperationRateLimitStoreResult> IncrementAsync(
        string key, TimeSpan duration, int maxCount)
    {
        if (maxCount <= 0)
        {
            return new OperationRateLimitStoreResult
            {
                IsAllowed = false,
                CurrentCount = 0,
                MaxCount = maxCount,
                RetryAfter = duration
            };
        }

        await using (var handle = await DistributedLock.TryAcquireAsync(
            $"OperationRateLimit:{key}", Options.LockTimeout))
        {
            if (handle == null)
            {
                throw new AbpException(
                    "Could not acquire distributed lock for operation rate limit. " +
                    "This is an infrastructure issue, not a rate limit violation.");
            }

            var cacheItem = await Cache.GetAsync(key);
            var now = new DateTimeOffset(Clock.Now.ToUniversalTime());

            if (cacheItem == null || now >= cacheItem.WindowStart.Add(duration))
            {
                cacheItem = new OperationRateLimitCacheItem { Count = 1, WindowStart = now };
                await Cache.SetAsync(key, cacheItem,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = duration
                    });

                return new OperationRateLimitStoreResult
                {
                    IsAllowed = true,
                    CurrentCount = 1,
                    MaxCount = maxCount
                };
            }

            if (cacheItem.Count >= maxCount)
            {
                var retryAfter = cacheItem.WindowStart.Add(duration) - now;
                return new OperationRateLimitStoreResult
                {
                    IsAllowed = false,
                    CurrentCount = cacheItem.Count,
                    MaxCount = maxCount,
                    RetryAfter = retryAfter
                };
            }

            cacheItem.Count++;
            var expiration = cacheItem.WindowStart.Add(duration) - now;
            await Cache.SetAsync(key, cacheItem,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration > TimeSpan.Zero ? expiration : duration
                });

            return new OperationRateLimitStoreResult
            {
                IsAllowed = true,
                CurrentCount = cacheItem.Count,
                MaxCount = maxCount
            };
        }
    }

    public virtual async Task<OperationRateLimitStoreResult> GetAsync(
        string key, TimeSpan duration, int maxCount)
    {
        if (maxCount <= 0)
        {
            return new OperationRateLimitStoreResult
            {
                IsAllowed = false,
                CurrentCount = 0,
                MaxCount = maxCount,
                RetryAfter = duration
            };
        }

        var cacheItem = await Cache.GetAsync(key);
        var now = new DateTimeOffset(Clock.Now.ToUniversalTime());

        if (cacheItem == null || now >= cacheItem.WindowStart.Add(duration))
        {
            return new OperationRateLimitStoreResult
            {
                IsAllowed = true,
                CurrentCount = 0,
                MaxCount = maxCount
            };
        }

        if (cacheItem.Count >= maxCount)
        {
            var retryAfter = cacheItem.WindowStart.Add(duration) - now;
            return new OperationRateLimitStoreResult
            {
                IsAllowed = false,
                CurrentCount = cacheItem.Count,
                MaxCount = maxCount,
                RetryAfter = retryAfter
            };
        }

        return new OperationRateLimitStoreResult
        {
            IsAllowed = true,
            CurrentCount = cacheItem.Count,
            MaxCount = maxCount
        };
    }

    public virtual async Task ResetAsync(string key)
    {
        await Cache.RemoveAsync(key);
    }
}
