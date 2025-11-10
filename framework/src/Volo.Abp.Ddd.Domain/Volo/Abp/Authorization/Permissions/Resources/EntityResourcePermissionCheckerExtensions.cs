using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class EntityResourcePermissionCheckerExtensions
{
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