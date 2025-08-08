using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI.Delegates;

public class ChatClientWithSystemMessage : DelegatingChatClient
{
    public string SystemMessage { get; private set; }

    public ChatClientWithSystemMessage(IChatClient innerClient, string systemMessage) : base(innerClient)
    {
        SystemMessage = systemMessage;
    }

    public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return base.GetResponseAsync(PrepareMessages(messages), options, cancellationToken);
    }

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return base.GetStreamingResponseAsync(PrepareMessages(messages), options, cancellationToken);
    }

    protected virtual List<ChatMessage> PrepareMessages(IEnumerable<ChatMessage> messages)
    {
        var messagesList = messages.ToList();

        if(messagesList.Any(x => x.Role == ChatRole.System))
        {
            // If there is a system message, skip it. It might be continued conversation.
            // No need to add a new one to prevent duplication.

            // If developer provided system message, then it's overridden, still skipping.

            // Logger.LogWarning("System message is not supported in ChatClientWithSystemMessage. Skipping.");

            return messagesList;
        }

        if(!SystemMessage.IsNullOrEmpty())
        {
            messagesList.Insert(0, new ChatMessage(ChatRole.System, SystemMessage));
        }

        return messagesList;
    }
}