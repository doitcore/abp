using System.Threading.Tasks;

namespace Volo.Abp.Authorization.Permissions.Resources;

public interface IResourcePermissionValueProvider
{
    string Name { get; }

    //TODO: Rename to GetResult? (CheckAsync throws exception by naming convention)
    Task<PermissionGrantResult> CheckAsync(ResourcePermissionValueCheckContext context);

    Task<MultiplePermissionGrantResult> CheckAsync(ResourcePermissionValuesCheckContext context);
}
