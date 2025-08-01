using Microsoft.Extensions.AI;

namespace Volo.Abp.AI;

public interface IChatClient<T> : IChatClient
    where T: class
{
    
}