using System;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI;

public class ChatClientConfiguration
{
    public string Name { get; }
    
    public ChatClientBuilder? Builder { get; set; }
    
    public BuilderConfigurerList BuilderConfigurers { get; }
    
    // TODO: Base chat client (for inheriting a chat client configuration from some other one) 

    public ChatClientConfiguration(string name)
    {
        Name = name;
        BuilderConfigurers = new BuilderConfigurerList();
    }

    public void ConfigureBuilder(Action<ChatClientBuilder> configureAction)
    {
        BuilderConfigurers.Add(configureAction);
    }
    
    public void ConfigureBuilder(string name, Action<ChatClientBuilder> configureAction)
    {
        BuilderConfigurers.Add(name, configureAction);
    }
}