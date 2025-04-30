using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace Volo.Abp.AI;

/* TODO:
 * - A factory service (IChatClientFactory) to create a IChatClient by given name
 *   (currently, we are using a keyed service collection to resolve the IChatClient)
 *   - We can also inject typed clients, like ICommentSummaryChatClient or IChatClient<CommentSummaryChatClient> (more practical to resolve)
 *     In this way, we can write specific extension methods to some clients, for prompts.
 * - 
 */

public class AbpAIModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<AbpAIChatClientOptions>();

        foreach (var chatClientConfig in options.ChatClients.Values)
        {
            if (chatClientConfig.Builder == null)
            {
                throw new AbpException("ChatClientBuilder is not properly configured. Set the Builder.");
            }

            foreach (var builderConfigurer in chatClientConfig.BuilderConfigurers)
            {
                builderConfigurer.Action(chatClientConfig.Builder);
            }

            context.Services.AddKeyedChatClient(
                AbpAIChatClientOptions.GetChatClientServiceKeyName(chatClientConfig.Name),
                provider => chatClientConfig.Builder.Build(provider)
            );
        }
        
        context.Services.TryAddTransient(typeof(IChatClient<>), typeof(TypedChatClient<>));
    }
}