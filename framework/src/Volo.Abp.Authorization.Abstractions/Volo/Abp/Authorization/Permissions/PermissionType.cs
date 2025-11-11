namespace Volo.Abp.Authorization.Permissions;

public enum PermissionType
{
    /// <summary>
    /// Based on user(roles/claims).
    /// </summary>
    UserBased = 0,

    /// <summary>
    /// Based on resource(entities).
    /// </summary>
    ResourceBased = 1
}
