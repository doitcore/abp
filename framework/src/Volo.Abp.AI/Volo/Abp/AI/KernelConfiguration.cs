using System;
using Microsoft.SemanticKernel;

namespace Volo.Abp.AI;

public class KernelConfiguration
{
    public string Name { get; }

    public IKernelBuilder? Builder { get; set; }

    public KernelBuilderConfigurerList BuilderConfigurers { get; }

    public KernelConfiguration(string name)
    {
        Name = name;
        BuilderConfigurers = new KernelBuilderConfigurerList();
    }

    public void ConfigureBuilder(Action<IKernelBuilder> configureAction)
    {
        BuilderConfigurers.Add(configureAction);
    }

    public void ConfigureBuilder(string name, Action<IKernelBuilder> configureAction)
    {
        BuilderConfigurers.Add(name, configureAction);
    }
}