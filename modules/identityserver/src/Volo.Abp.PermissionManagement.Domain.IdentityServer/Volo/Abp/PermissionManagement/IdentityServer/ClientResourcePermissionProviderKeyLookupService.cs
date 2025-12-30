using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions.Resources;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IdentityServer.Clients;
using Volo.Abp.IdentityServer.Localization;
using Volo.Abp.Localization;

namespace Volo.Abp.PermissionManagement.IdentityServer;

public class ClientResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => ClientResourcePermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IClientFinder ClientFinder { get; }

    public ClientResourcePermissionProviderKeyLookupService(IClientFinder clientFinder)
    {
        ClientFinder = clientFinder;
        DisplayName = LocalizableString.Create<AbpIdentityServerResource>(nameof(ClientResourcePermissionProviderKeyLookupService));
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var clients = await ClientFinder.SearchAsync(filter, page);
        return clients.Select(x => new ResourcePermissionProviderKeyInfo(x.Id.ToString(), x.ClientId)).ToList();
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        var ids = keys
            .Select(key => Guid.TryParse(key, out var id) ? (Guid?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToArray();
        var clients = await ClientFinder.SearchByIdsAsync(ids.ToArray());
        return clients.Select(x => new ResourcePermissionProviderKeyInfo(x.Id.ToString(), x.ClientId)).ToList();
    }
}
