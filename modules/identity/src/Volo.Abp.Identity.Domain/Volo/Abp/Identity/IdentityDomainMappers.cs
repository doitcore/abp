using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Abp.Users;

namespace Volo.Abp.Identity;

[Mapper]
public partial class IdentityUserToUserEtoMapper : MapperBase<IdentityUser, UserEto>
{
    public override partial UserEto Map(IdentityUser source);
    public override partial void Map(IdentityUser source, UserEto destination);
}

[Mapper]
public partial class IdentityClaimTypeToIdentityClaimTypeEtoMapper : MapperBase<IdentityClaimType, IdentityClaimTypeEto>
{
    public override partial IdentityClaimTypeEto Map(IdentityClaimType source);
    public override partial void Map(IdentityClaimType source, IdentityClaimTypeEto destination);
}

[Mapper]
public partial class IdentityRoleToIdentityRoleEtoMapper : MapperBase<IdentityRole, IdentityRoleEto>
{
    public override partial IdentityRoleEto Map(IdentityRole source);
    public override partial void Map(IdentityRole source, IdentityRoleEto destination);
}

[Mapper]
public partial class OrganizationUnitToOrganizationUnitEtoMapper : MapperBase<OrganizationUnit, OrganizationUnitEto>
{
    public override partial OrganizationUnitEto Map(OrganizationUnit source);
    public override partial void Map(OrganizationUnit source, OrganizationUnitEto destination);
}
