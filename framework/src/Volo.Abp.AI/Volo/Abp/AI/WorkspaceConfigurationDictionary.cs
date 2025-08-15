using System;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class WorkspaceConfigurationDictionary : Dictionary<string, WorkspaceConfiguration>
{
    public void Configure<TWorkSpace>(Action<WorkspaceConfiguration> configureAction)
        where TWorkSpace : class
    {
        Configure(WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>(), configureAction);
    }

    public void Configure(string name, Action<WorkspaceConfiguration> configureAction)
    {
        if (!TryGetValue(name, out var configuration))
        {
            configuration = new WorkspaceConfiguration(name);
            this[name] = configuration;
        }

        configureAction(configuration);
    }
}
