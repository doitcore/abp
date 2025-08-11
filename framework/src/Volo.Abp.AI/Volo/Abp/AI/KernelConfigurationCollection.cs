using System;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class KernelConfigurationDictionary : Dictionary<string, KernelConfiguration>
{
    public static string DefaultKernelName => "Default";

    public void ConfigureDefault(Action<KernelConfiguration> configureAction) =>
        Configure(DefaultKernelName, configureAction);

    public void Configure<TWorkSpace>(Action<KernelConfiguration> configureAction)
    {
        Configure(typeof(TWorkSpace), configureAction);
    }

    public void Configure(Type workspaceType, Action<KernelConfiguration> configureAction)
    {
        Configure(
            WorkspaceNameAttribute.GetWorkspaceName(workspaceType),
            configureAction
        );
    }

    public void Configure(string name, Action<KernelConfiguration> configureAction)
    {
        if (!TryGetValue(name, out var configuration))
        {
            configuration = new KernelConfiguration(name);
            this[name] = configuration;
        }

        configureAction(configuration);
    }
}


