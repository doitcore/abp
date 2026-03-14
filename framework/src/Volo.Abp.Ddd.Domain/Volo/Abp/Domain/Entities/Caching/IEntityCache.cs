using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Volo.Abp.Domain.Entities.Caching;

public interface IEntityCache<TEntityCacheItem, in TKey>
    where TEntityCacheItem : class
{
    /// <summary>
    /// Gets the entity with given <paramref name="id"/>,
    /// or returns null if the entity was not found.
    /// </summary>
    Task<TEntityCacheItem?> FindAsync(TKey id);

    /// <summary>
    /// Gets multiple entities with the given <paramref name="ids"/>.
    /// Returns a list where each entry corresponds to the given id in the same order.
    /// An entry will be null if the entity was not found for the corresponding id.
    /// </summary>
    Task<List<TEntityCacheItem?>> FindManyAsync(IEnumerable<TKey> ids);

    /// <summary>
    /// Gets the entity with given <paramref name="id"/>,
    /// or throws <see cref="EntityNotFoundException"/> if the entity was not found.
    /// </summary>
    [ItemNotNull] 
    Task<TEntityCacheItem> GetAsync(TKey id);

    /// <summary>
    /// Gets multiple entities with the given <paramref name="ids"/>.
    /// Returns a list where each entry corresponds to the given id in the same order.
    /// Throws <see cref="EntityNotFoundException"/> if any entity was not found.
    /// </summary>
    Task<List<TEntityCacheItem>> GetManyAsync(IEnumerable<TKey> ids);
}
