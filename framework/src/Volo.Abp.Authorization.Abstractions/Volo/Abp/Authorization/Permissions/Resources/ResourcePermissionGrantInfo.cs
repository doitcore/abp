using JetBrains.Annotations;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class ResourcePermissionGrantInfo : PermissionGrantInfo
{
    public string ResourceName { get; }

    public string ResourceKey { get; }

    public ResourcePermissionGrantInfo([NotNull] string name, bool isGranted, string resourceName, string resourceKey, string? providerName = null, string? providerKey = null)
        : base(name, isGranted, providerName, providerKey)
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }
}
