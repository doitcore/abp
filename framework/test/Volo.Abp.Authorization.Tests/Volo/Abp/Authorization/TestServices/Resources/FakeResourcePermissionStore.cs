using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Authorization.Permissions.Resources;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Authorization.TestServices.Resources;

public class FakeResourcePermissionStore : IResourcePermissionStore, ITransientDependency
{
    public Task<bool> IsGrantedAsync(string name, string resourceName, string resourceKey, string providerName, string providerKey)
    {
        throw new System.NotImplementedException();
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names, string resourceName, string resourceKey, string providerName, string providerKey)
    {
        throw new System.NotImplementedException();
    }

    public Task<MultiplePermissionGrantResult> GetPermissionsAsync(string resourceName, string resourceKey)
    {
        throw new System.NotImplementedException();
    }

    public Task<string[]> GetGrantedPermissionsAsync(string resourceName, string resourceKey)
    {
        throw new System.NotImplementedException();
    }

    public Task<string[]> GetGrantedResourceKeysAsync(string resourceName, string name)
    {
        throw new System.NotImplementedException();
    }
}
