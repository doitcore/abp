using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.PermissionManagement.Localization;

namespace Volo.Abp.PermissionManagement;

public class TestPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddResourcePermission("PR1", "Volo.Abp.Identity.IdentityRole", L("Role Resource Permission1"));
        context.AddResourcePermission("PR2", "Volo.Abp.Identity.IdentityRole", L("Role Resource Permission2"));
        context.AddResourcePermission("PR3", "Volo.Abp.Identity.IdentityRole", L("Role Resource Permission3"));
        context.AddResourcePermission("PR4", "Volo.Abp.Identity.IdentityRole", L("Role Resource Permission4"));
        context.AddResourcePermission("PR5", "Volo.Abp.Identity.IdentityRole", L("Role Resource Permission5"));

        context.AddResourcePermission("PU1", "Volo.Abp.Identity.IdentityUser", L("User Resource Permission1"));
        context.AddResourcePermission("PU2", "Volo.Abp.Identity.IdentityUser", L("User Resource Permission2"));
        context.AddResourcePermission("PU3", "Volo.Abp.Identity.IdentityUser", L("User Resource Permission3"));
        context.AddResourcePermission("PU4", "Volo.Abp.Identity.IdentityUser", L("User Resource Permission4"));
        context.AddResourcePermission("PU5", "VVolo.Abp.Identity.IdentityUser", L("User Resource Permission5"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpPermissionManagementResource>(name);
    }
}
