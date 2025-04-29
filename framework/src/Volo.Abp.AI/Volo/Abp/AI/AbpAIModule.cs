using Microsoft.Extensions.DependencyInjection;
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
                throw new AbpException("ChatClientBuilder is not properly configured. Set the Builder.");
            }

            foreach (var builderConfigurer in chatClientConfig.BuilderConfigurers)
            {
                builderConfigurer.Action(chatClientConfig.Builder);
            }
            
            context.Services.AddKeyedChatClient(chatClientConfig.Name, provider => chatClientConfig.Builder.Build(provider));
        }
    }
}