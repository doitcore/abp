using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.PermissionManagement;

public interface IResourcePermissionManagementProvider : ISingletonDependency //TODO: Consider to remove this pre-assumption
{
    string Name { get; }

    Task<PermissionValueProviderGrantInfo> CheckAsync(
        [NotNull] string name,
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        [NotNull] string providerName,
        [NotNull] string providerKey
    );

    Task<MultiplePermissionValueProviderGrantInfo> CheckAsync(
        [NotNull] string[] names,
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        [NotNull] string providerName,
        [NotNull] string providerKey
    );

    Task SetAsync(
        [NotNull] string name,
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        [NotNull] string providerKey,
        bool isGranted
    );
}
