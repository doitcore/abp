using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.Authorization.Permissions.Resources;

public interface IResourcePermissionChecker
{
    Task<bool> IsGrantedAsync(
        string permissionName,
        string resourceName,
        string resourceKey
    );
    
    Task<IDictionary<string, bool>> GetPermissionsAsync(
        string resourceName,
        string resourceKey
    );
    
    Task<string[]> GetGrantedPermissionsAsync(
        string resourceName,
        string resourceKey
    );
}