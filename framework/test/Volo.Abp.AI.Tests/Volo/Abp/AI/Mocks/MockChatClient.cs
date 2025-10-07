using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI.Mocks;

public class MockChatClient : IChatClient
{
    public const int StreamingResponseParts = 12;
    public void Dispose()
    {

    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default)
    {
        var responseMessages = messages.ToList();
        responseMessages.Add(new ChatMessage(ChatRole.Assistant, "This is a mock response."));
        return Task.FromResult(new ChatResponse
        {
           Messages = responseMessages,
        });
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        return null;
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < StreamingResponseParts; i++)
        {
            await Task.Delay(25, cancellationToken);
            yield return new ChatResponseUpdate
            {
                RawRepresentation = "This is a mock streaming response part " + (i + 1)
            };
        }
    }
}
