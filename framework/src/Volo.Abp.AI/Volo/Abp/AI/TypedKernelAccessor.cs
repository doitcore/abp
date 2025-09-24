using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public class TypedKernelAccessor<TWorkSpace> : IKernelAccessor<TWorkSpace>
    where TWorkSpace : class
{
    public Kernel? Kernel { get; }

    public TypedKernelAccessor(IServiceProvider serviceProvider)
    {
        Kernel = serviceProvider.GetKeyedService<Kernel>(
            AbpAIOptions.GetKernelServiceKeyName(
                WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()));
    }
}


