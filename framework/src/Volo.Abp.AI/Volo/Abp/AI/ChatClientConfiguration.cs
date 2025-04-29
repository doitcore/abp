using Microsoft.Extensions.AI;

namespace Volo.Abp.AI;

public class ChatClientConfiguration
{
    public string Name { get; }
    
    public ChatClientBuilder? Builder { get; set; }
    
    public BuilderConfigurerList BuilderConfigurers { get; } = new();

    public ChatClientConfiguration(string name)
    {
        Name = name;
    }
}