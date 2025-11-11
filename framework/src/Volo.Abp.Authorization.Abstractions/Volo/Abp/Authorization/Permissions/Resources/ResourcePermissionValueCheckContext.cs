using System.Security.Claims;
using JetBrains.Annotations;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class ResourcePermissionValueCheckContext : PermissionValueCheckContext
{
    [NotNull]
    public string ResourceName { get; }

    [NotNull]
    public string ResourceKey { get; }

    public ResourcePermissionValueCheckContext([NotNull] PermissionDefinition permission, string resourceName, string resourceKey)
        : this(permission, null, resourceName, resourceKey)
    {
    }

    public ResourcePermissionValueCheckContext([NotNull] PermissionDefinition permission, ClaimsPrincipal? principal, string resourceName, string resourceKey)
        : base(permission, principal)
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }
}
