namespace Volo.Abp.AI;

public class AbpAIChatClientOptions
{
    public const string ChatClientServiceKeyNamePrefix = "Abp.AI.ChatClient_";
    
    public ChatClientConfigurationDictionary ChatClients { get; } = new();

    public static string GetChatClientServiceKeyName(string name)
    {
        return $"{ChatClientServiceKeyNamePrefix}{name}";
    }
}