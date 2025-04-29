using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Volo.Abp.AI;

public class ChatClientConfigurationCollection
{
    private readonly ConcurrentDictionary<string, ChatClientConfiguration> _chatClients = new();

    public void Configure(string name, Action<ChatClientConfiguration> configureAction)
    {
        if (!_chatClients.TryGetValue(name, out var configuration))
        {
            configuration = new ChatClientConfiguration(name);
            _chatClients[name] = configuration;
        }

        configureAction(configuration);
    }

    internal IEnumerable<ChatClientConfiguration> GetAll()
    {
        return _chatClients.Values;
    }
}