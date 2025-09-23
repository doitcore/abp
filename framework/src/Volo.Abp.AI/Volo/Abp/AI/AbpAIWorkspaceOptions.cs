using System.Collections.Generic;

namespace Volo.Abp.AI;

public class AbpAIWorkspaceOptions //TODO: Rename to AbpAIOptions
{
    public HashSet<string> ConfiguredWorkspaceNames { get; } = new();
}