using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace Volo.Abp.AI;

public class AbpAIModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<AbpAIOptions>();

        foreach (var chatClientConfig in options.ChatClients.Values)
        {
            if (chatClientConfig.Builder == null)
            {
                throw new AbpException("ChatClientBuilder is not properly configured. Set the Builder property.");
            }

            foreach (var builderConfigurer in chatClientConfig.BuilderConfigurers)
            {
                builderConfigurer.Action(chatClientConfig.Builder);
            }

            context.Services.AddKeyedChatClient(
                AbpAIOptions.GetChatClientServiceKeyName(chatClientConfig.Name),
                provider => chatClientConfig.Builder.Build(provider)
            );

            if (chatClientConfig.Name == ChatClientConfigurationDictionary.DefaultChatClientName)
            {
                context.Services.AddTransient<IChatClient>(sp => sp.GetRequiredKeyedService<IChatClient>(
                        AbpAIOptions.GetChatClientServiceKeyName(chatClientConfig.Name)
                    )
                );
            }
        }
        
        context.Services.TryAddTransient(typeof(IChatClient<>), typeof(TypedChatClient<>));
    }
}