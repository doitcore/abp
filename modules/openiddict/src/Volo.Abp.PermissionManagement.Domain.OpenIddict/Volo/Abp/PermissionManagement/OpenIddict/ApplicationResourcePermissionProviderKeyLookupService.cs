using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Volo.Abp.Authorization.Permissions.Resources;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.OpenIddict.Applications;

namespace Volo.Abp.PermissionManagement.OpenIddict;

public class ApplicationResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => ClientResourcePermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IApplicationFinder ApplicationFinder { get; }

    public ApplicationResourcePermissionProviderKeyLookupService(IApplicationFinder applicationFinder)
    {
        ApplicationFinder = applicationFinder;
        DisplayName = LocalizableString.Create<OpenIddictResources>(nameof(ApplicationResourcePermissionProviderKeyLookupService));
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var users = await ApplicationFinder.SearchAsync(filter, page);
        return users.Select(u => new ResourcePermissionProviderKeyInfo(u.Id.ToString(), u.ClientId)).ToList();
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        var ids = keys
            .Select(key => Guid.TryParse(key, out var id) ? (Guid?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToArray();
        var users = await ApplicationFinder.SearchByIdsAsync(ids.ToArray());
        return users.Select(u => new ResourcePermissionProviderKeyInfo(u.Id.ToString(), u.ClientId)).ToList();
    }
}
