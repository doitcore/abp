using System;
using System.Linq;

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

    public static string GetChatClientName<TChatClient>()
    {
        return GetChatClientName(typeof(TChatClient));
    }
    
    public static string GetChatClientName(Type chatClientType)
    {
        var chatClientNameAttribute = chatClientType
            .GetCustomAttributes(true)
            .OfType<ChatClientNameAttribute>()
            .FirstOrDefault();

        if (chatClientNameAttribute != null)
        {
            return chatClientNameAttribute.Name;
        }

        return chatClientType.FullName!;
    }
}