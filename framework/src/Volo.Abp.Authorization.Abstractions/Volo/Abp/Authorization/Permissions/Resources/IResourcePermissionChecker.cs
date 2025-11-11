using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Volo.Abp.Authorization.Permissions.Resources;

public interface IResourcePermissionChecker
{
    Task<bool> IsGrantedAsync([NotNull] string name, string resourceName, string resourceKey);

    Task<bool> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, [NotNull] string name, string resourceName, string resourceKey);

    Task<MultiplePermissionGrantResult> IsGrantedAsync([NotNull] string[] names, string resourceName, string resourceKey);

    Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal? claimsPrincipal, [NotNull] string[] names, string resourceName, string resourceKey);
}
