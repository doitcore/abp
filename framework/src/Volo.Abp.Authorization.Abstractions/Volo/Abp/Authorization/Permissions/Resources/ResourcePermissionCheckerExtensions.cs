using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class ResourcePermissionCheckerExtensions
{
    public static Task<bool> IsGrantedAsync<TResource>(
        this IResourcePermissionChecker resourcePermissionChecker,
        string permissionName,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionChecker.IsGrantedAsync(
            permissionName,
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }
    
    public static Task<IDictionary<string, bool>> GetPermissionsAsync<TResource>(
        this IResourcePermissionChecker resourcePermissionChecker,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionChecker.GetPermissionsAsync(
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }
    
    public static Task<string[]> GetGrantedPermissionsAsync<TResource>(
        this IResourcePermissionChecker resourcePermissionChecker,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionChecker.GetGrantedPermissionsAsync(
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }
    
    public static Task<string[]> GetGrantedResourceKeysAsync<TResource>(
        this IResourcePermissionChecker resourcePermissionChecker,
        TResource resource,
        string permissionName
    )
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNull(resource, nameof(resource));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));

        return resourcePermissionChecker.GetGrantedResourceKeysAsync(
            typeof(TResource).FullName!,
            permissionName
        );
    }
}