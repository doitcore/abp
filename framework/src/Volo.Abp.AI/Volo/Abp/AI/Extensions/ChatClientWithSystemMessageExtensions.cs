using Microsoft.Extensions.AI;
using Volo.Abp.AI.Delegates;

namespace Volo.Abp.AI.Extensions;

public static class ChatClientWithSystemMessageExtensions
{
    public static ChatClientBuilder UseSystemMessage(this ChatClientBuilder builder, string systemMessage)
    {
        return builder.Use(client => new ChatClientWithSystemMessage(client, systemMessage));
    }
}