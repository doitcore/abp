using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class ResourcePermissionPopulator : ITransientDependency
{
    protected IPermissionDefinitionManager PermissionDefinitionManager { get; }

    protected IResourcePermissionChecker ResourcePermissionChecker { get; }

    protected IResourcePermissionStore ResourcePermissionStore { get; }

    public ResourcePermissionPopulator(
        IPermissionDefinitionManager permissionDefinitionManager,
        IResourcePermissionChecker resourcePermissionChecker,
        IResourcePermissionStore resourcePermissionStore)
    {
        PermissionDefinitionManager = permissionDefinitionManager;
        ResourcePermissionChecker = resourcePermissionChecker;
        ResourcePermissionStore = resourcePermissionStore;
    }

    public virtual async Task PopulateAsync<TResource>(TResource resource, string resourceName)
        where TResource : IHasResourcePermissions
    {
        await PopulateAsync([resource], resourceName);
    }

    public virtual async Task PopulateAsync<TResource>(List<TResource> resources, string resourceName)
        where TResource : IHasResourcePermissions
    {
        Check.NotNull(resources, nameof(resources));
        Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName));

        var resopurcePermissionNames = (await PermissionDefinitionManager.GetResourcePermissionsAsync())
            .Where(x => x.ResourceName == resourceName)
            .Select(x => x.Name)
            .ToArray();

        foreach (var resource in resources)
        {
            var results = await ResourcePermissionChecker.IsGrantedAsync(resopurcePermissionNames, resourceName, resource.GetResourceKey());
            foreach (var resopurcePermission in resopurcePermissionNames)
            {
                if(resource.ResourcePermissions == null)
                {
                     ObjectHelper.TrySetProperty(resource, x => x.ResourcePermissions, () => new Dictionary<string, bool>());
                }
                var hasPermission = results.Result.TryGetValue(resopurcePermission, out var granted) && granted == PermissionGrantResult.Granted;
                resource.ResourcePermissions![resopurcePermission] = hasPermission;
            }
        }
    }
}
