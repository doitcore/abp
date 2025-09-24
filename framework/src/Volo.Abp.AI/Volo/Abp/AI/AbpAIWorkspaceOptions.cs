using System.Collections.Generic;

namespace Volo.Abp.AI;

public class AbpAIWorkspaceOptions
{
    public HashSet<string> ConfiguredWorkspaceNames { get; } = new();
}