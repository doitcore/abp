using System.Collections.Generic;
using System.Security.Claims;
using JetBrains.Annotations;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class ResourcePermissionValuesCheckContext : PermissionValuesCheckContext
{
    [NotNull]
    public string ResourceName { get; }

    [NotNull]
    public string ResourceKey { get; }

    public ResourcePermissionValuesCheckContext([NotNull] PermissionDefinition permission,string resourceName, string resourceKey)
        : this([permission], null, resourceName, resourceKey)
    {

    }


    public ResourcePermissionValuesCheckContext([NotNull] PermissionDefinition permission, ClaimsPrincipal? principal, string resourceName, string resourceKey)
        : this([permission], principal, resourceName, resourceKey)
    {

    }

    public ResourcePermissionValuesCheckContext([NotNull] List<PermissionDefinition> permissions, string resourceName, string resourceKey)
        : this(permissions, null, resourceName, resourceKey)
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    public ResourcePermissionValuesCheckContext([NotNull] List<PermissionDefinition> permissions, ClaimsPrincipal? principal, string resourceName, string resourceKey)
        : base(permissions, principal)
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }
}
