using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class EntityResourcePermissionCheckerExtensions
{
    /// <summary>
    /// Checks if the specified permission is granted for the given entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <param name="entity">The entity for which the permission is being checked.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the permission is granted.</returns>
    public static Task<bool> IsGrantedAsync<TEntity>(
        this IResourcePermissionChecker resourcePermissionChecker,
        string permissionName,
        TEntity entity
    )
        where TEntity : class, IEntity
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));
        Check.NotNull(entity, nameof(entity));

        return resourcePermissionChecker.IsGrantedAsync(
            permissionName,
            typeof(TEntity).FullName!,
            entity.GetKeys().JoinAsString(",")
        );
    }

    /// <summary>
    /// Retrieves a dictionary of permissions and their granted status for the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="entity">The entity for which the permissions are being retrieved.</param>
    /// <returns>A dictionary where the keys are permission names and the values are booleans indicating whether the permission is granted.</returns>
    public static Task<IDictionary<string, bool>> GetPermissionsAsync<TEntity>(
        this IResourcePermissionChecker resourcePermissionChecker,
        TEntity entity
    )
        where TEntity : class, IEntity
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNull(entity, nameof(entity));

        return resourcePermissionChecker.GetPermissionsAsync(
            typeof(TEntity).FullName!,
            entity.GetKeys().JoinAsString(",")
        );
    }

    /// <summary>
    /// Retrieves an array of granted permissions for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="entity">The entity for which the permissions are being checked.</param>
    /// <returns>An array of granted permission names as strings.</returns>
    public static Task<string[]> GetGrantedPermissionsAsync<TEntity>(
        this IResourcePermissionChecker resourcePermissionChecker,
        TEntity entity
    )
        where TEntity : class, IEntity
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNull(entity, nameof(entity));

        return resourcePermissionChecker.GetGrantedPermissionsAsync(
            typeof(TEntity).FullName!,
            entity.GetKeys().JoinAsString(",")
        );
    }

    /// <summary>
    /// Retrieves an array of granted entity IDs for a specific permission.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>An array of entity IDs (of type <typeparamref name="TKey"/>) for which the permission is granted.</returns>
    public async static Task<TKey[]> GetGrantedEntityIdsAsync<TEntity, TKey>(
        this IResourcePermissionChecker resourcePermissionChecker,
        string permissionName
    )
        where TEntity : class, IEntity<TKey>
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));

        var keys = await resourcePermissionChecker.GetGrantedResourceKeysAsync(
            typeof(TEntity).FullName!,
            permissionName
        );
        
        return keys
            .Select(x => Convert.ChangeType(x, typeof(TKey)))
            .Cast<TKey>()
            .ToArray();
    }
}