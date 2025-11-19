using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Authorization.Permissions.Resources;

public class ResourcePermissionPopulator : ITransientDependency
{
    protected IPermissionDefinitionManager PermissionDefinitionManager { get; }

    protected IResourcePermissionChecker ResourcePermissionChecker { get; }

    public ResourcePermissionPopulator(IPermissionDefinitionManager permissionDefinitionManager, IResourcePermissionChecker resourcePermissionChecker)
    {
        PermissionDefinitionManager = permissionDefinitionManager;
        ResourcePermissionChecker = resourcePermissionChecker;
    }

    public virtual async Task PopulateAsync(IHasResourcePermissions resource, string resourceName, string resourceKey)
    {
        Check.NotNull(resource, nameof(resource));
        Check.NotNull(resource.ResourcePermissions, nameof(resource.ResourcePermissions));
        Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName));
        Check.NotNullOrWhiteSpace(resourceKey, nameof(resourceKey));

        var resopurcePermissionNames = (await PermissionDefinitionManager.GetResourcePermissionsAsync())
            .Where(x => x.ResourceName == resourceName)
            .Select(x => x.Name)
            .ToArray();

        var results = await ResourcePermissionChecker.IsGrantedAsync(resopurcePermissionNames, resourceName, resourceKey);
        foreach (var resopurcePermission in resopurcePermissionNames)
        {
            var hasPermission = results.Result.TryGetValue(resopurcePermission, out var granted) &&
                                granted == PermissionGrantResult.Granted;
            resource.ResourcePermissions[resopurcePermission] = hasPermission;
        }
    }
}
