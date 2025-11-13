using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.Authorization.Permissions.Resources;

public static class ResourcePermissionCheckerExtensions
{
    /// <summary>
    /// Checks if a specific permission is granted for a resource with a given key.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <param name="resource">The resource instance to check permission for.</param>
    /// <param name="resourceKey">The unique key identifying the resource instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the permission is granted.</returns>
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

    /// <summary>
    /// Retrieves a dictionary of permissions and their granted statuses for a specific resource.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="resourcePermissionChecker">The resource permission checker instance.</param>
    /// <param name="resource">The resource instance for which permissions are being checked.</param>
    /// <param name="resourceKey">The unique key identifying the resource instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary where keys are permission names and values indicate whether the corresponding permission is granted.</returns>
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
}
