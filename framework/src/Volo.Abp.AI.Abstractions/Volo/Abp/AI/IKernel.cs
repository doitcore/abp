using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public interface IKernel<TWorkSpace>
    where TWorkSpace : class
{
    Kernel Kernel { get; }
}