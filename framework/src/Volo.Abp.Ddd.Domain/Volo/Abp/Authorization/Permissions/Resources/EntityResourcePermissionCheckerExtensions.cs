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
    public static Task<bool> IsGrantedAsync<TEntity>(this IResourcePermissionChecker resourcePermissionChecker, string permissionName, TEntity entity)
        where TEntity : class, IEntity
    {
        Check.NotNull(resourcePermissionChecker, nameof(resourcePermissionChecker));
        Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName));
        Check.NotNull(entity, nameof(entity));

        return resourcePermissionChecker.IsGrantedAsync(
            permissionName,
            entity,
            entity.GetResourceKey()
        );
    }
}
