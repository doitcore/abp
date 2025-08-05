using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Volo.Abp.AI.Delegates;

public class ChatClientWithTools : DelegatingChatClient
{
    public IList<AIFunction> Tools { get; private set; }

    public ChatToolMode? ToolMode { get; private set; }

    public bool? AllowMultipleToolCalls { get; private set; }

    public ChatClientWithTools(IChatClient innerClient, IEnumerable<AIFunction> tools, ChatToolMode? toolMode = null, bool? allowMultipleToolCalls = null)
        : base(innerClient)
    {
        Tools = tools.ToList();
        ToolMode = toolMode;
        AllowMultipleToolCalls = allowMultipleToolCalls;
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
        options ??= new ChatOptions();

        options.Tools ??= new List<AITool>();

        foreach (var tool in Tools)
        {
            options.Tools.Add(tool);
        }

        if (ToolMode is not null)
        {
            options.ToolMode = ToolMode;
        }

        if (AllowMultipleToolCalls is not null)
        {
            options.AllowMultipleToolCalls = AllowMultipleToolCalls.Value;
        }

        return options;
    }
}