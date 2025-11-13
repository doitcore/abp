using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Application.Services;

namespace Volo.Abp.PermissionManagement;

public interface IPermissionAppService : IApplicationService
{
    Task<GetPermissionListResultDto> GetAsync([NotNull] string providerName, [NotNull] string providerKey);

    Task<GetPermissionListResultDto> GetByGroupAsync([NotNull] string groupName, [NotNull] string providerName, [NotNull] string providerKey);

    Task UpdateAsync([NotNull] string providerName, [NotNull] string providerKey, UpdatePermissionsDto input);

    Task<GetResourcePermissionListResultDto> GetAsync([NotNull] string resourceName, [NotNull] string resourceKey, [NotNull] string providerName, [NotNull] string providerKey);

    Task UpdateAsync([NotNull] string resourceName, [NotNull] string resourceKey, [NotNull] string providerName, [NotNull] string providerKey, UpdatePermissionsDto input);
}
