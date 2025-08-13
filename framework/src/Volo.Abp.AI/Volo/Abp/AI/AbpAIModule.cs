using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Volo.Abp.AI;

[DependsOn(
    typeof(AbpAIAbstractionsModule)
)]
public class AbpAIModule : AbpModule
{

    public const string DefaultWorkspaceName = "Default";

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<AbpAIOptions>();

        foreach (var workspaceConfig in options.Workspaces.Values)
        {
            if (workspaceConfig.ChatClient?.Builder is null)
            {
                continue;
            }

            foreach (var builderConfigurer in workspaceConfig.ChatClient.BuilderConfigurers)
            {
                builderConfigurer.Action(workspaceConfig.ChatClient.Builder!);
            }

            context.Services.AddKeyedChatClient(
                AbpAIOptions.GetChatClientServiceKeyName(workspaceConfig.ChatClient.Name),
                provider => workspaceConfig.ChatClient.Builder!.Build(provider)
            );

            if (workspaceConfig.ChatClient.Name == DefaultWorkspaceName)
            {
                context.Services.AddTransient<IChatClient>(sp => sp.GetRequiredKeyedService<IChatClient>(
                        AbpAIOptions.GetChatClientServiceKeyName(workspaceConfig.ChatClient.Name)
                    )
                );
            }
        }

        context.Services.TryAddTransient(typeof(IChatClient<>), typeof(TypedChatClient<>));

        foreach (var workspaceConfig in options.Workspaces.Values)
        {
            if (workspaceConfig.Kernel?.Builder is null)
            {
                continue;
            }

            foreach (var builderConfigurer in workspaceConfig.Kernel.BuilderConfigurers)
            {
                builderConfigurer.Action(workspaceConfig.Kernel.Builder!);
            }

            context.Services.AddKeyedSingleton<Kernel>(
                AbpAIOptions.GetKernelServiceKeyName(workspaceConfig.Kernel.Name),
                (provider, _) => workspaceConfig.Kernel.Builder!.Build());

            if (workspaceConfig.Kernel.Name == DefaultWorkspaceName)
            {
                context.Services.AddSingleton<Kernel>(sp => sp.GetRequiredKeyedService<Kernel>(
                        AbpAIOptions.GetKernelServiceKeyName(workspaceConfig.Kernel.Name)
                    )
                );
            }
        }

        context.Services.TryAddTransient(typeof(IKernelAccessor<>), typeof(TypedKernelAccessor<>));
    }
}