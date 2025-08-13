using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public interface IKernelAccessor<TWorkSpace>
    where TWorkSpace : class
{
    Kernel? Kernel { get; }
}