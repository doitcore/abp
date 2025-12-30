using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Volo.Abp.Threading;

/// <summary>
/// Per-key asynchronous lock.
/// https://stackoverflow.com/a/31194647
/// </summary>
public static class KeyedLock
{
    private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlims = new();

    public static async Task<IDisposable> LockAsync(object key)
    {
        return await LockAsync(key, CancellationToken.None).ConfigureAwait(false);
    }

    public static async Task<IDisposable> LockAsync(object key, CancellationToken cancellationToken)
    {
        var semaphore = GetOrCreate(key);
        try
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            var toDispose = DecrementRefAndMaybeRemove(key);
            toDispose?.Dispose();
            throw;
        }
        return new Releaser(key);
    }

    public static async Task<IDisposable?> TryLockAsync(object key)
    {
        return await TryLockAsync(key, default, CancellationToken.None).ConfigureAwait(false);
    }

    public static async Task<IDisposable?> TryLockAsync(object key, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var semaphore = GetOrCreate(key);
        bool acquired;
        try
        {
            if (timeout == default)
            {
                acquired = await semaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                acquired = await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            var toDispose = DecrementRefAndMaybeRemove(key);
            toDispose?.Dispose();
            throw;
        }

        if (acquired)
        {
            return new Releaser(key);
        }

        var toDisposeOnFail = DecrementRefAndMaybeRemove(key);
        toDisposeOnFail?.Dispose();

        return null;
    }

    private static SemaphoreSlim GetOrCreate(object key)
    {
        RefCounted<SemaphoreSlim> item;
        lock (SemaphoreSlims)
        {
            if (SemaphoreSlims.TryGetValue(key, out item!))
            {
                ++item.RefCount;
            }
            else
            {
                item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                SemaphoreSlims[key] = item;
            }
        }
        return item.Value;
    }

    private sealed class RefCounted<T>(T value)
    {
        public int RefCount { get; set; } = 1;

        public T Value { get; } = value;
    }

    private sealed class Releaser(object key) : IDisposable
    {
        public void Dispose()
        {
            RefCounted<SemaphoreSlim> item;
            lock (SemaphoreSlims)
            {
                if (!SemaphoreSlims.TryGetValue(key, out item!))
                {
                    return;
                }
                --item.RefCount;
            }
            item.Value.Release();

            bool shouldDispose = false;
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out var current) && ReferenceEquals(current, item))
                {
                    if (item.RefCount == 0)
                    {
                        SemaphoreSlims.Remove(key);
                        shouldDispose = true;
                    }
                }
            }

            if (shouldDispose)
            {
                item.Value.Dispose();
            }
        }
    }

    private static SemaphoreSlim? DecrementRefAndMaybeRemove(object key)
    {
        RefCounted<SemaphoreSlim>? itemToDispose = null;
        lock (SemaphoreSlims)
        {
            if (SemaphoreSlims.TryGetValue(key, out var item))
            {
                --item.RefCount;
                if (item.RefCount == 0)
                {
                    SemaphoreSlims.Remove(key);
                    itemToDispose = item;
                }
            }
        }
        return itemToDispose?.Value;
    }
}
