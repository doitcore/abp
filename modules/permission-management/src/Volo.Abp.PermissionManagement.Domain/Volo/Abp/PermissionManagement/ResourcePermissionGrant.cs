using System;
using JetBrains.Annotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.PermissionManagement;

//TODO: To aggregate root?
public class ResourcePermissionGrant : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    [NotNull]
    public virtual string Name { get; protected set; }

    [NotNull]
    public virtual string ProviderName { get; protected set; }

    [CanBeNull]
    public virtual string ProviderKey { get; protected internal set; }

    [NotNull]
    public virtual string ResourceName { get; protected set; }

    [NotNull]
    public virtual string ResourceKey { get; protected set; }

    protected ResourcePermissionGrant()
    {

    }

    public ResourcePermissionGrant(
        Guid id,
        [NotNull] string name,
        [NotNull] string providerName,
        [CanBeNull] string providerKey,
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        Guid? tenantId = null)
    {
        Check.NotNull(name, nameof(name));

        Id = id;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        ProviderName = Check.NotNullOrWhiteSpace(providerName, nameof(providerName));
        ProviderKey = providerKey;
        ResourceName = Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName));
        ResourceKey = Check.NotNullOrWhiteSpace(resourceKey, nameof(resourceKey));
        TenantId = tenantId;
    }
}
