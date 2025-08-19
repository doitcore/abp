using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.TenantManagement.Web.Pages.TenantManagement.Tenants;

namespace Volo.Abp.TenantManagement.Web;

[Mapper]
public partial class TenantDtoToTenantInfoModelMapper
    : MapperBase<TenantDto, EditModalModel.TenantInfoModel>
{
    public override partial EditModalModel.TenantInfoModel Map(TenantDto source);

    public override partial void Map(TenantDto source, EditModalModel.TenantInfoModel destination);
}

[Mapper]
public partial class TenantInfoModelToTenantCreateDtoMapper
    : MapperBase<EditModalModel.TenantInfoModel, TenantCreateDto>
{
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.Id))]
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(TenantCreateDto.AdminEmailAddress))]
    [MapperIgnoreTarget(nameof(TenantCreateDto.AdminPassword))]
    public override partial TenantCreateDto Map(EditModalModel.TenantInfoModel source);

    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.Id))]
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(TenantCreateDto.AdminEmailAddress))]
    [MapperIgnoreTarget(nameof(TenantCreateDto.AdminPassword))]
    public override partial void Map(EditModalModel.TenantInfoModel source, TenantCreateDto destination);
}

[Mapper]
public partial class TenantInfoModelToTenantUpdateDtoMapper
    : MapperBase<EditModalModel.TenantInfoModel, TenantUpdateDto>
{
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.Id))]
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(TenantUpdateDto.ConcurrencyStamp))]
    public override partial TenantUpdateDto Map(EditModalModel.TenantInfoModel source);

    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.Id))]
    [MapperIgnoreSource(nameof(EditModalModel.TenantInfoModel.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(TenantUpdateDto.ConcurrencyStamp))]
    public override partial void Map(EditModalModel.TenantInfoModel source, TenantUpdateDto destination);
}