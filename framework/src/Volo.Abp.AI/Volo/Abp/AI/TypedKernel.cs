using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public class TypedKernel<TWorkSpace> : IKernel<TWorkSpace>
    where TWorkSpace : class
{
    public Kernel Kernel { get; }

    public TypedKernel(IServiceProvider serviceProvider)
    {
        Kernel = serviceProvider.GetRequiredKeyedService<Kernel>(
            AbpAIOptions.GetKernelServiceKeyName(
                WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()));
    }
}


