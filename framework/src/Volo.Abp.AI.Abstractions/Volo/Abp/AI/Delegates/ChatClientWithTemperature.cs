using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI.Delegates;

public class ChatClientWithTemperature : DelegatingChatClient
{
    private readonly float _temperature;

    public ChatClientWithTemperature(IChatClient innerClient, float temperature)
        : base(innerClient)
    {
        _temperature = temperature;
    }

    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return base.GetResponseAsync(
            messages,
            BuildChatOptions(options),
            cancellationToken
        );
    }

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return base.GetStreamingResponseAsync(
            messages,
            BuildChatOptions(options),
            cancellationToken
        );
    }
    
    private ChatOptions BuildChatOptions(ChatOptions? options)
    {
        if (options?.Temperature != null)
        {
            return options;
        }

        options ??= new ChatOptions();

        options.Temperature ??= _temperature;
        
        return options;
    }
}