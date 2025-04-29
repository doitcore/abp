using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Volo.Abp.AI;

public class AbpAIModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<AbpAIOptions>();

        foreach (var chatClientConfig in options.ChatClients.GetAll())
        {
            if (chatClientConfig.Builder == null)
            {
                throw new AbpException("ChatClientBuilder is not properly configured. Set the Builder.");
            }

            context.Services.AddKeyedChatClient(chatClientConfig.Name, provider => chatClientConfig.Builder.Build(provider));
        }
    }
}