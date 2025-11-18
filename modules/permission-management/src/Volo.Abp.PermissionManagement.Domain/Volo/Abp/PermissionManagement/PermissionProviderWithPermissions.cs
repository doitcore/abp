using System.Collections.Generic;

namespace Volo.Abp.PermissionManagement;

public class PermissionProviderWithPermissions
{
    public string ProviderName { get; set; }

    public string ProviderKey { get; set; }

    public string ProviderDisplayName { get; set; }

    public List<string> Permissions { get; set; }

    public PermissionProviderWithPermissions(string providerName, string providerKey, string providerDisplayName)
    {
        ProviderName = providerName;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        Permissions = new List<string>();
    }
}
