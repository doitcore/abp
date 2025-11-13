using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.PermissionManagement;

public interface IResourcePermissionManager
{
    Task<PermissionWithGrantedProviders> GetAsync(
        string permissionName,
        string resourceName,
        string resourceKey,
        string providerName,
        string providerKey
    );

    Task<MultiplePermissionWithGrantedProviders> GetAsync(
        string[] permissionNames,
        string resourceName,
        string resourceKey,
        string providerName,
        string providerKey
    );

    Task<List<PermissionWithGrantedProviders>> GetAllAsync(
        string resourceName,
        string resourceKey,
        string providerName,
        string providerKey
    );

    Task SetAsync(
        string permissionName,
        string resourceName,
        string resourceKey,
        string providerName,
        string providerKey,
        bool isGranted
    );
}
