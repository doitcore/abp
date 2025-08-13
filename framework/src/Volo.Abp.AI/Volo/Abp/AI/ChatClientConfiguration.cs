using System;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI;

public class ChatClientConfiguration
{    
    public ChatClientBuilder? Builder { get; set; }
    
    public BuilderConfigurerList BuilderConfigurers { get; } = new();
    
    // TODO: Base chat client (for inheriting a chat client configuration from some other one) 

    public void ConfigureBuilder(Action<ChatClientBuilder> configureAction)
    {
        BuilderConfigurers.Add(configureAction);
    }
    
    public void ConfigureBuilder(string name, Action<ChatClientBuilder> configureAction)
    {
        BuilderConfigurers.Add(name, configureAction);
    }
}