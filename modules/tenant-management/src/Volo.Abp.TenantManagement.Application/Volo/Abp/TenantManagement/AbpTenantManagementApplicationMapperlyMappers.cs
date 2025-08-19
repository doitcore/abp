using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace Volo.Abp.TenantManagement.Application.Volo.Abp.TenantManagement;

[Mapper]
public partial class TenantToTenantDtoMapper 
    : MapperBase<Tenant, TenantDto>
{
    [MapperIgnoreSource(nameof(Tenant.EntityVersion))]
    [MapperIgnoreSource(nameof(Tenant.ConnectionStrings))]
    [MapperIgnoreSource(nameof(Tenant.IsDeleted))]
    [MapperIgnoreSource(nameof(Tenant.NormalizedName))]
    [MapperIgnoreSource(nameof(Tenant.DeleterId))]
    [MapperIgnoreSource(nameof(Tenant.DeletionTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModifierId))]
    [MapperIgnoreSource(nameof(Tenant.CreationTime))]
    [MapperIgnoreSource(nameof(Tenant.CreatorId))]
    public override partial TenantDto Map(Tenant source);

    [MapperIgnoreSource(nameof(Tenant.EntityVersion))]
    [MapperIgnoreSource(nameof(Tenant.ConnectionStrings))]
    [MapperIgnoreSource(nameof(Tenant.IsDeleted))]
    [MapperIgnoreSource(nameof(Tenant.NormalizedName))]
    [MapperIgnoreSource(nameof(Tenant.DeleterId))]
    [MapperIgnoreSource(nameof(Tenant.DeletionTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tenant.LastModifierId))]
    [MapperIgnoreSource(nameof(Tenant.CreationTime))]
    [MapperIgnoreSource(nameof(Tenant.CreatorId))]
    public override partial void Map(Tenant source, TenantDto destination);
}