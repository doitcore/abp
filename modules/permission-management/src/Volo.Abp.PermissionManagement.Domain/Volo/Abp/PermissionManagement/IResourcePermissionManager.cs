using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.PermissionManagement;

public interface IResourcePermissionManager
{
    Task<PermissionWithGrantedProviders> GetAsync(
        string permissionName,
        string providerName,
        string providerKey,
        string resourceName,
        string resourceKey
    );
    
    Task<MultiplePermissionWithGrantedProviders> GetAsync(
        string[] permissionNames,
        string provideName,
        string providerKey,
        string resourceName,
        string resourceKey
    );
    
    Task<List<PermissionWithGrantedProviders>> GetAllAsync(
        string providerName,
        string providerKey,
        string resourceName,
        string resourceKey
    );
    
    Task SetAsync(
        string permissionName,
        string providerName,
        string providerKey,
        string resourceName,
        string resourceKey,
        bool isGranted
    );
}