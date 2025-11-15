using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.PermissionManagement.Localization;

namespace Volo.Abp.PermissionManagement;

public class TestPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddResourcePermission("AbpIdentity.Resource.ChangeName", "Volo.Abp.IdentityUser", L("OnlyProviderPermissons"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpPermissionManagementResource>(name);
    }
}
