using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public class TypedKernel<TWorkSpace> : IKernel<TWorkSpace>
    where TWorkSpace : class
{
    public Kernel InnerKernel { get; }

    public TypedKernel(IServiceProvider serviceProvider)
    {
        InnerKernel = serviceProvider.GetRequiredKeyedService<Kernel>(
            AbpAIOptions.GetKernelServiceKeyName(
                WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()));
    }
}


