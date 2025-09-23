using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Volo.Abp.Modularity;

namespace Volo.Abp.AI;

[DependsOn(
    typeof(AbpAIAbstractionsModule)
)]
public class AbpAIModule : AbpModule
{
    public const string DefaultWorkspaceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var options = context.Services.ExecutePreConfiguredActions<AbpAIOptions>();

        context.Services.Configure<AbpAIWorkspaceOptions>(workspaceOptions =>
        {
            workspaceOptions.ConfiguredWorkspaceNames.AddIfNotContains(
                options.Workspaces.Select(x => x.Key)
            );
        });
        
        //TODO: Refactor & merge foreach loops

        foreach (var workspaceConfig in options.Workspaces.Values)
        {
            if (workspaceConfig.ChatClient.Builder is null)
            {
                continue;
            }

            foreach (var builderConfigurer in workspaceConfig.ChatClient.BuilderConfigurers)
            {
                builderConfigurer.Action(workspaceConfig.ChatClient.Builder);
            }

            var serviceName = AbpAIOptions.GetChatClientServiceKeyName(workspaceConfig.Name);
            
            context.Services.AddKeyedChatClient(
                serviceName,
                provider => workspaceConfig.ChatClient.Builder.Build(provider),
                ServiceLifetime.Transient
            );

            if (workspaceConfig.Name == DefaultWorkspaceName)
            {
                context.Services.AddTransient<IChatClient>(
                    sp => sp.GetRequiredKeyedService<IChatClient>(serviceName)
                );
            }
        }

        context.Services.TryAddTransient(typeof(IChatClient<>), typeof(TypedChatClient<>));

        foreach (var workspaceConfig in options.Workspaces.Values)
        {
            if (workspaceConfig.Kernel.Builder is null)
            {
                continue;
            }

            foreach (var builderConfigurer in workspaceConfig.Kernel.BuilderConfigurers)
            {
                builderConfigurer.Action(workspaceConfig.Kernel.Builder!);
            }

            // TODO: Check if we can use transient instead of singleton for Kernel
            context.Services.AddKeyedTransient<Kernel>(
                AbpAIOptions.GetKernelServiceKeyName(workspaceConfig.Name),
                (provider, _) => workspaceConfig.Kernel.Builder!.Build());

            if (workspaceConfig.Name == DefaultWorkspaceName)
            {
                context.Services.AddTransient<Kernel>(sp => sp.GetRequiredKeyedService<Kernel>(
                        AbpAIOptions.GetKernelServiceKeyName(workspaceConfig.Name)
                    )
                );
            }

            if (workspaceConfig.ChatClient?.Builder is null)
            {
                context.Services.AddKeyedTransient<IChatClient>(
                    AbpAIOptions.GetChatClientServiceKeyName(workspaceConfig.Name),
                    (sp, _) => sp.GetKeyedService<Kernel>(AbpAIOptions.GetKernelServiceKeyName(workspaceConfig.Name))?
                        .GetRequiredService<IChatClient>() 
                            ?? throw new InvalidOperationException("Kernel or IChatClient not found with workspace name: " + workspaceConfig.Name)
                );
            }
        }

        context.Services.TryAddTransient(typeof(IKernelAccessor<>), typeof(KernelAccessor<>));
    }
}