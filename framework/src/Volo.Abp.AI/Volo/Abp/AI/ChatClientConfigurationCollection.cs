using System;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class ChatClientConfigurationDictionary : Dictionary<string, ChatClientConfiguration>
{
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
