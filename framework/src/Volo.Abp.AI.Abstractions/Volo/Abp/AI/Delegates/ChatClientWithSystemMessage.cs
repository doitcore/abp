using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI.Delegates;

public class ChatClientWithSystemMessage : DelegatingChatClient
{
    private readonly string _systemMessage;

    public ChatClientWithSystemMessage(IChatClient innerClient, string systemMessage)
        : base(innerClient)
    {
        _systemMessage = systemMessage;
    }

    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return base.GetResponseAsync(
            BuildMessageList(messages),
            options,
            cancellationToken
        );
    }

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return base.GetStreamingResponseAsync(
            BuildMessageList(messages),
            options,
            cancellationToken
        );
    }
    
    private List<ChatMessage> BuildMessageList(IEnumerable<ChatMessage> messages)
    {
        var messagesList = messages.ToList();

        if (messagesList.Count <= 0 || messagesList[0].Role != ChatRole.System)
        {
            messagesList.AddFirst(new ChatMessage(ChatRole.System, _systemMessage));
        }

        return messagesList;
    }
}