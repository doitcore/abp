using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp.AI;

public class TypedChatClient<T> : DelegatingChatClient, IChatClient<T>
    where T : class
{
    public TypedChatClient(IServiceProvider serviceProvider)
        : base(
            serviceProvider.GetRequiredKeyedService<IChatClient>(
                AbpAIOptions.GetChatClientServiceKeyName(
                    ChatClientNameAttribute.GetChatClientName<T>()))
        )
    {
    }
}