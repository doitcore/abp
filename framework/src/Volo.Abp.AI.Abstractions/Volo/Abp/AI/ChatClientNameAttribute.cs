using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Volo.Abp.AI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class ChatClientNameAttribute : Attribute
{
    public string Name { get; }

    public ChatClientNameAttribute(string name)
    {
        Check.NotNull(name, nameof(name));

        Name = name;
    }

    private static readonly ConcurrentDictionary<Type, string> _nameCache = new();

    public static string GetChatClientName<TChatClient>()
    {
        return GetChatClientName(typeof(TChatClient));
    }
    
    public static string GetChatClientName(Type chatClientType)
    {
        return _nameCache.GetOrAdd(chatClientType, type =>
        {
            var chatClientNameAttribute = type
                .GetCustomAttributes(true)
                .OfType<ChatClientNameAttribute>()
                .FirstOrDefault();

            return chatClientNameAttribute?.Name ?? type.FullName!;
        });
    }
}