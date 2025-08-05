using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using  Volo.Abp.AI.Delegates;

namespace Volo.Abp.AI.Extensions;

public static class ChatClientWithSystemMessageExtensions 
{
    public static ChatClientBuilder UseSystemMessage(this ChatClientBuilder builder, string systemMessage)
    {
        return builder.Use(chatClient => new ChatClientWithSystemMessage(chatClient, systemMessage));
    }
}