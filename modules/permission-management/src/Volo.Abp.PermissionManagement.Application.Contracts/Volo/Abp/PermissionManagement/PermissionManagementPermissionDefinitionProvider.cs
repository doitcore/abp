using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.PermissionManagement.Localization;

namespace Volo.Abp.PermissionManagement;

public class PermissionManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(PermissionManagementPermissions.GroupName, L("Permission:PermissionManagement"));
        var manageResourcePermissions = group.AddPermission(PermissionManagementPermissions.ManageResourcePermissions, L("Permission:ManageResourcePermissions"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpPermissionManagementResource>(name);
    }
}
