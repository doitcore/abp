using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.Mapperly;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.TenantManagement.Domain.Volo.Abp.TenantManagement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TenantToTenantConfigurationMapper
    : MapperBase<Tenant, TenantConfiguration>
{
    [MapperIgnoreTarget(nameof(TenantConfiguration.EditionId))]
    [MapperIgnoreTarget(nameof(TenantConfiguration.IsActive))]
    public override partial TenantConfiguration Map(Tenant source);

    [MapperIgnoreTarget(nameof(TenantConfiguration.EditionId))]
    [MapperIgnoreTarget(nameof(TenantConfiguration.IsActive))]
    public override partial void Map(Tenant source, TenantConfiguration destination);

	protected virtual ConnectionStrings Map(List<TenantConnectionString> source)
	{
		var connStrings = new ConnectionStrings();

		if (source == null)
		{
			return connStrings;
		}

		foreach (var connectionString in source)
		{
			connStrings[connectionString.Name] = connectionString.Value;
		}

		return connStrings;
	}
}

[Mapper]
public partial class TenantToTenantEtoMapper
    : MapperBase<Tenant, TenantEto>
{
    [MapperIgnoreSource(nameof(Tenant.ConnectionStrings))]
    [MapperIgnoreSource(nameof(Tenant.IsDeleted))]
    [MapperIgnoreSource(nameof(Tenant.NormalizedName))]
    [MapperIgnoreSource(nameof(Tenant.DeleterId))]
    [MapperIgnoreSource(nameof(Tenant.DeletionTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModifierId))]
    [MapperIgnoreSource(nameof(Tenant.CreationTime))]
    [MapperIgnoreSource(nameof(Tenant.CreatorId))]
    [MapperIgnoreSource(nameof(Tenant.ExtraProperties))]
    [MapperIgnoreSource(nameof(Tenant.ConcurrencyStamp))]
    public override partial TenantEto Map(Tenant source);

    [MapperIgnoreSource(nameof(Tenant.ConnectionStrings))]
    [MapperIgnoreSource(nameof(Tenant.IsDeleted))]
    [MapperIgnoreSource(nameof(Tenant.NormalizedName))]
    [MapperIgnoreSource(nameof(Tenant.DeleterId))]
    [MapperIgnoreSource(nameof(Tenant.DeletionTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModifierId))]
    [MapperIgnoreSource(nameof(Tenant.CreationTime))]
    [MapperIgnoreSource(nameof(Tenant.CreatorId))]
    [MapperIgnoreSource(nameof(Tenant.ExtraProperties))]
    [MapperIgnoreSource(nameof(Tenant.ConcurrencyStamp))]
    public override partial void Map(Tenant source, TenantEto destination);
}