using System;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class AbpAIOptions
{
    public const string ChatClientServiceKeyNamePrefix = "Abp.AI.ChatClient_";
    public const string KernelServiceKeyNamePrefix = "Abp.AI.Kernel_";
    
    public WorkspaceConfigurationDictionary Workspaces { get; } = new();

    public static string GetChatClientServiceKeyName(string name)
    {
        return $"{ChatClientServiceKeyNamePrefix}{name}";
    }

    public static string GetKernelServiceKeyName(string name)
    {
        return $"{KernelServiceKeyNamePrefix}{name}";
    }
}

public class WorkspaceConfiguration
{
    public ChatClientConfiguration? ChatClient { get; set; }
    public KernelConfiguration? Kernel { get; set; }
}

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
            configuration = new WorkspaceConfiguration();
            this[name] = configuration;
        }

        configureAction(configuration);
    }
}