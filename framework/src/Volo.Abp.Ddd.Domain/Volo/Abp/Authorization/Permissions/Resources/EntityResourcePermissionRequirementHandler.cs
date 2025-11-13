using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class EntityResourcePermissionRequirementHandler : AuthorizationHandler<ResourcePermissionRequirement, IEntity>
{
    protected readonly IResourcePermissionChecker PermissionChecker;

    public EntityResourcePermissionRequirementHandler(IResourcePermissionChecker permissionChecker)
    {
        PermissionChecker = permissionChecker;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, IEntity? resource)
    {
        if (resource == null)
        {
            return;
        }

        var resourceName = resource.GetType().FullName!;
        var resourceKey = resource.GetKeys().JoinAsString(",");
        if (await PermissionChecker.IsGrantedAsync(context.User, requirement.PermissionName, resourceName, resourceKey))
        {
            context.Succeed(requirement);
        }
    }
}
