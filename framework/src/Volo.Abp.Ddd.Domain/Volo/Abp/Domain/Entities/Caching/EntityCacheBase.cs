using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Uow;

namespace Volo.Abp.Domain.Entities.Caching;

public abstract class EntityCacheBase<TEntity, TEntityCacheItem, TKey> :
    IEntityCache<TEntityCacheItem, TKey>,
    ILocalEventHandler<EntityChangedEventData<TEntity>>
    where TEntity : Entity<TKey>
    where TEntityCacheItem : class
{
    protected IReadOnlyRepository<TEntity, TKey> Repository { get; }
    protected IDistributedCache<EntityCacheItemWrapper<TEntityCacheItem>, TKey> Cache { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    protected EntityCacheBase(
        IReadOnlyRepository<TEntity, TKey> repository,
        IDistributedCache<EntityCacheItemWrapper<TEntityCacheItem>, TKey> cache,
        IUnitOfWorkManager unitOfWorkManager)
    {
        Repository = repository;
        Cache = cache;
        UnitOfWorkManager = unitOfWorkManager;
    }

    public virtual async Task<TEntityCacheItem?> FindAsync(TKey id)
    {
        return (await Cache.GetOrAddAsync(
            id,
            async () =>
            {
                if (HasObjectExtensionInfo())
                {
                    Repository.EnableTracking();
                }

                return MapToCacheItem(await Repository.FindAsync(id))!;
            }))?.Value;
    }

    public virtual async Task<List<TEntityCacheItem?>> FindManyAsync(IEnumerable<TKey> ids)
    {
        var idArray = ids.ToArray();
        var cacheItems = await GetOrAddManyCacheItemsAsync(idArray);

        return idArray
            .Select(id => cacheItems.FirstOrDefault(x => EqualityComparer<TKey>.Default.Equals(x.Key, id)).Value?.Value)
            .ToList();
    }

    public virtual async Task<TEntityCacheItem> GetAsync(TKey id)
    {
        return (await Cache.GetOrAddAsync(
            id,
            async () =>
            {
                if (HasObjectExtensionInfo())
                {
                    Repository.EnableTracking();
                }

                return MapToCacheItem(await Repository.GetAsync(id))!;
            }))!.Value!;
    }

    public virtual async Task<List<TEntityCacheItem>> GetManyAsync(IEnumerable<TKey> ids)
    {
        var idArray = ids.ToArray();
        var cacheItems = await GetOrAddManyCacheItemsAsync(idArray);

        return idArray
            .Select(id =>
            {
                var cacheItem = cacheItems.FirstOrDefault(x => EqualityComparer<TKey>.Default.Equals(x.Key, id)).Value?.Value;
                if (cacheItem == null)
                {
                    throw new EntityNotFoundException(typeof(TEntity), id);
                }

                return cacheItem;
            })
            .ToList();
    }

    protected virtual async Task<KeyValuePair<TKey, EntityCacheItemWrapper<TEntityCacheItem>?>[]> GetOrAddManyCacheItemsAsync(TKey[] ids)
    {
        return await Cache.GetOrAddManyAsync(
            ids,
            async missingKeys =>
            {
                if (HasObjectExtensionInfo())
                {
                    Repository.EnableTracking();
                }

                var missingKeyArray = missingKeys.ToArray();
                var entities = await Repository.GetListAsync(
                    x => missingKeyArray.Contains(x.Id)
                );

                return missingKeyArray
                    .Select(key =>
                    {
                        var entity = entities.FirstOrDefault(e => EqualityComparer<TKey>.Default.Equals(e.Id, key));
                        return new KeyValuePair<TKey, EntityCacheItemWrapper<TEntityCacheItem>>(
                            key,
                            MapToCacheItem(entity)!
                        );
                    })
                    .ToList();
            });
    }

    protected virtual bool HasObjectExtensionInfo()
    {
        return typeof(IHasExtraProperties).IsAssignableFrom(typeof(TEntity)) &&
               ObjectExtensionManager.Instance.GetOrNull(typeof(TEntity)) != null;
    }

    protected abstract EntityCacheItemWrapper<TEntityCacheItem>? MapToCacheItem(TEntity? entity);

    public async Task HandleEventAsync(EntityChangedEventData<TEntity> eventData)
    {
        /* Why we are using double remove:
         * First Cache.RemoveAsync drops the cache item in a unit of work.
         * Some other application / thread may read the value from database and put it to the cache again
         * before the UOW completes.
         * The second Cache.RemoveAsync drops the cache item after the database transaction is complete.
         * Only the second Cache.RemoveAsync may not be enough if the application crashes just after the UOW completes.
         */

        await Cache.RemoveAsync(eventData.Entity.Id);

        if(UnitOfWorkManager.Current != null)
        {
            await Cache.RemoveAsync(eventData.Entity.Id, considerUow: true);
        }
    }
}
