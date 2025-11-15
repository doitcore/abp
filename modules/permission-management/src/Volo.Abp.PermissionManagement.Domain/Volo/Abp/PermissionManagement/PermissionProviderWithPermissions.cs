using System.Collections.Generic;
using JetBrains.Annotations;

namespace Volo.Abp.PermissionManagement;

public class PermissionProviderWithPermissions
{
    public string ProviderName { get; }

    public string ProviderKey { get; }

    public List<string> Permissions { get; set; }

    public PermissionProviderWithPermissions(string providerName, string providerKey)
    {
        ProviderName = providerName;
        ProviderKey = providerKey;
        Permissions = new List<string>();
    }
}
