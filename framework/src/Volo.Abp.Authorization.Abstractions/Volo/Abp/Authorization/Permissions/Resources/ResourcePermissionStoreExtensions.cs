using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class ResourcePermissionStoreExtensions
{
    /// <summary>
    /// Checks if a specific permission is granted for a resource with a given key.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission checker instance.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <param name="resource">The resource instance to check permission for.</param>
    /// <param name="resourceKey">The unique key identifying the resource instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the permission is granted.</returns>
    public static Task<bool> IsGrantedAsync<TResource>(
        this IResourcePermissionStore resourcePermissionStore,
        string permissionName,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionStore.IsGrantedAsync(
            permissionName,
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }

    /// <summary>
    /// Retrieves a dictionary of permissions and their granted statuses for a specific resource.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission checker instance.</param>
    /// <param name="resource">The resource instance for which permissions are being checked.</param>
    /// <param name="resourceKey">The unique key identifying the resource instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary where keys are permission names and values indicate whether the corresponding permission is granted.</returns>
    public static Task<IDictionary<string, bool>> GetPermissionsAsync<TResource>(
        this IResourcePermissionStore resourcePermissionStore,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionStore.GetPermissionsAsync(
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }

    /// <summary>
    /// Retrieves the list of granted permissions for a specific resource with a given key.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission checker instance.</param>
    /// <param name="resource">The resource instance to retrieve permissions for.</param>
    /// <param name="resourceKey">The unique key identifying the resource instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of strings representing the granted permissions.</returns>
    public static Task<string[]> GetGrantedPermissionsAsync<TResource>(
        this IResourcePermissionStore resourcePermissionStore,
        TResource resource,
        object resourceKey
    )
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionStore.GetGrantedPermissionsAsync(
            typeof(TResource).FullName!,
            resourceKey.ToString()!
        );
    }

    /// <summary>
    /// Retrieves the keys of the resources granted a specific permission.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionStore">The resource permission checker instance.</param>
    /// <param name="resource">The resource instance to check granted permissions for.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of strings representing the granted resource keys.</returns>
    public static Task<string[]> GetGrantedResourceKeysAsync<TResource>(
        this IResourcePermissionStore resourcePermissionStore,
        TResource resource,
        string permissionName
    )
    {
        Check.NotNull(resourcePermissionStore, nameof(resourcePermissionStore));
        Check.NotNull(resource, nameof(resource));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));

        return resourcePermissionStore.GetGrantedResourceKeysAsync(
            typeof(TResource).FullName!,
            permissionName
        );
    }
}
