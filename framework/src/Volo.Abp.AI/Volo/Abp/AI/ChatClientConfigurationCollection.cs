using System;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class ChatClientConfigurationDictionary : Dictionary<string, ChatClientConfiguration>
{
    public static string DefaultChatClientName => "Default";
    
    public void ConfigureDefault(Action<ChatClientConfiguration> configureAction) =>
        Configure(DefaultChatClientName, configureAction);

    public void Configure<T>(Action<ChatClientConfiguration> configureAction)
    {
        Configure(typeof(T), configureAction);
    }
    
    public void Configure(Type chatClientType, Action<ChatClientConfiguration> configureAction)
    {
        Configure(
            ChatClientNameAttribute.GetChatClientName(chatClientType),
            configureAction
        );
    }
    
    public void Configure(string name, Action<ChatClientConfiguration> configureAction)
    {
        if (!this.TryGetValue(name, out var configuration))
        {
            configuration = new ChatClientConfiguration(name);
            this[name] = configuration;
        }

        configureAction(configuration);
    }
}