using Volo.Abp.Reflection;

namespace Volo.Abp.PermissionManagement;

public static class PermissionManagementPermissions
{
    public const string GroupName = "PermissionManagement";

    public const string ManageResourcePermissions = GroupName + ".ManageResourcePermissions";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(PermissionManagementPermissions));
    }
}
