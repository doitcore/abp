using System.Collections.Generic;

namespace Volo.Abp.Authorization.Permissions.Resources;

public interface IHasResourcePermissions
{
    public Dictionary<string, bool> ResourcePermissions { get; }

    string GetResourceKey();
}
