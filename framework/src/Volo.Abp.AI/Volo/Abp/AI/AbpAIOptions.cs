namespace Volo.Abp.AI;

public class AbpAIOptions //TODO: Rename to AbpAIWorkspaceOptions
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