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
using Volo.Abp.OpenIddict.Localization;

namespace Volo.Abp.PermissionManagement.OpenIddict;

public class ApplicationResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => ClientResourcePermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IApplicationFinder ApplicationFinder { get; }

    public ApplicationResourcePermissionProviderKeyLookupService(IApplicationFinder applicationFinder)
    {
        ApplicationFinder = applicationFinder;
        DisplayName = LocalizableString.Create<AbpOpenIddictResource>(nameof(ApplicationResourcePermissionProviderKeyLookupService));
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var applications = await ApplicationFinder.SearchAsync(filter, page);
        return applications.Select(x => new ResourcePermissionProviderKeyInfo(x.Id.ToString(), x.ClientId)).ToList();
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        var ids = keys
            .Select(key => Guid.TryParse(key, out var id) ? (Guid?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToArray();
        var applications = await ApplicationFinder.SearchByIdsAsync(ids.ToArray());
        return applications.Select(x => new ResourcePermissionProviderKeyInfo(x.Id.ToString(), x.ClientId)).ToList();
    }
}
