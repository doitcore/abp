using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Volo.Abp.AI.Delegates;

namespace Volo.Abp.AI.Extensions;

public static class ChatClientWithToolsExtensions 
{
    public static ChatClientBuilder UseTools(
        this ChatClientBuilder builder,
        IEnumerable<AIFunction> tools,
        ChatToolMode? toolMode = null,
        bool? allowMultipleToolCalls = null)
    {
        return builder.Use(chatClient
            => new ChatClientWithTools(chatClient, tools, toolMode, allowMultipleToolCalls));
    }
}