using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class EntityResourcePermissionStoreExtensions
{
    /// <summary>
    /// Retrieves an array of granted permissions for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission store instance.</param>
    /// <param name="entity">The entity for which the permissions are being checked.</param>
    /// <returns>An array of granted permission names as strings.</returns>
    public static Task<string[]> GetGrantedPermissionsAsync<TEntity>(
        this IResourcePermissionStore resourcePermissionStore,
        TEntity entity
    )
        where TEntity : class, IEntity
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNull(entity, nameof(entity));

        return resourcePermissionStore.GetGrantedPermissionsAsync(
            typeof(TEntity).FullName!,
            entity.GetKeys().JoinAsString(",")
        );
    }

    /// <summary>
    /// Retrieves an array of granted entity IDs for a specific permission.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission store instance.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>An array of entity IDs (of type <typeparamref name="TKey"/>) for which the permission is granted.</returns>
    public static async Task<TKey[]> GetGrantedEntityIdsAsync<TEntity, TKey>(
        this IResourcePermissionStore resourcePermissionStore,
        string permissionName
    )
        where TEntity : class, IEntity<TKey>
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));

        var keys = await resourcePermissionStore.GetGrantedResourceKeysAsync(
            typeof(TEntity).FullName!,
            permissionName
        );

        return keys
            .Select(x => Convert.ChangeType(x, typeof(TKey)))
            .Cast<TKey>()
            .ToArray();
    }
}
