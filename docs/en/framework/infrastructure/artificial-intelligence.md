# Artificial Intelligence
ABP Framework provides integration for AI capabilities to your application by using Microsoft's AI stacks by using abstractions and workspaces. The main purpose of this integration is to provide a consistent way to use AI capabilities and managing different AI providers, models and configurations by using workspaces.

ABP Framework doesn't implement any AI providers or models, it only provides the abstractions by using Microsoft's packages such as [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai) and [Microsoft.SemanticKernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/).

ABP allows you to define a default configuration for across the application and also allows you to define isolated configurations for different different purposes by using workspaces. A workspace allows you to configure isolated AI configurations for a named scope. You can resolve AI services for a specific workspace when you need to use them.

## Installation

> This package is not included by default. Install it to enable AI features.

It is suggested to use the ABP CLI to install the package. Open a command line window in the folder of the project (.csproj file) and type the following command:

```bash
abp add-package Volo.Abp.AI
```

## Usage

Since ABP supports both `Microsoft.Extensions.AI` and `Microsoft.SemanticKernel`. Microsoft provides `IChatClient` interface abstraction for different chat client integrations. ABP respects it and uses it by default. Also `Kernel` object is used by `Microsoft.SemanticKernel` to execute AI capabilities. ABP respects it and uses it by default.

You can use both of them in your application by resolving `IChatClient` or `IKernelAccessor` services from the [service provider](../fundamentals/dependency-injection.md). `IChatClient` is the original interface from `Microsoft.Extensions.AI` but `IKernelAccessor` is a custom service that is used to access the `Kernel` object from `Microsoft.SemanticKernel`.

### Microsoft.Extensions.AI

You can resolve `IChatClient` to access configured chat client from your service and use it directly.

```csharp
public class MyService
{
    private readonly IChatClient _chatClient;
    public MyService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        return await _chatClient.GetResponseAsync(prompt);
    }
}
```

You can also resolve `IChatClientAccessor` to access the `IChatClient` optionally configured scenarios such as developing a module or a service that may use AI capabilities **optionally**.


```csharp
public class MyService
{
    private readonly IChatClientAccessor _chatClientAccessor;
    public MyService(IChatClientAccessor chatClientAccessor)
    {
        _chatClientAccessor = chatClientAccessor;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var chatClient = _chatClientAccessor.ChatClient;
        if (chatClient is null)
        {
            return "No chat client configured";
        }
        return await chatClient.GetResponseAsync(prompt);
    }
}
```

### Microsoft.SemanticKernel

Semantic Kernel can be used by resolving `IKernelAccessor` service that carries the `Kernel` instance. Kernel might be null if no workspace is configured. You should check the kernel before using it.

```csharp
public class MyService
{
    private readonly IKernelAccessor _kernelAccessor;
    public MyService(IKernelAccessor kernelAccessor)
    {
        _kernelAccessor = kernelAccessor;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var kernel = _kernelAccessor.Kernel;
        if (kernel is null)
        {
            return "No kernel configured";
        }
        return await kernel.InvokeAsync(prompt);
    }
}
```

### Workspaces

Workspaces are a way to configure isolated AI configurations for a named scope. You can define a workspace by decorating a class with the `WorkspaceNameAttribute` attribute that carries the workspace name.
- Workspace names must be unique.
- Workspace names cannot contain spaces _(use underscores or camelCase)_.
- Workspace names are case-sensitive.

```csharp
using Volo.Abp.AI;

[WorkspaceName("CommentSummarization")]
public class CommentSummarization
{
}
```

> [!NOTE]
> If you don't specify the workspace name, the full name of the class will be used as the workspace name.

You can resolve generic versions of `IChatClient`, `IChatClientAccessor` or `IKernelAccessor` services for a specific workspace as generic arguments. If Chat Client or Kernel is not configured for a workspace, you will get `null` from the accessor services. You should check the accessor before using it. This applies only for specified workspaces. Another workspace may have a configured Chat Client or Kernel.

```csharp
public class MyService
{
    private readonly IChatClientAccessor<CommentSummarization> _chatClientAccessor;
    public MyService(IChatClientAccessor<CommentSummarization> chatClientAccessor)
    {
        _chatClientAccessor = chatClientAccessor;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var chatClient = _chatClientAccessor.ChatClient;
        if (chatClient is null)
        {
            return "No chat client configured for 'CommentSummarization' workspace";
        }
        return await chatClient.GetResponseAsync(prompt);
    }
}
```

## Configuration

`AbpAIWorkspaceOptions` configuration is used to configure AI workspaces and their configurations. You can configure the default workspace and also configure isolated workspaces by using the this options class.It has to be configured **before the services are configured** in the `PreConfigure` method of your module class. It is important since the services are registered after the configuration is applied.

- `AbpAIWorkspaceOptions` has a `Workspaces` property that is type of `WorkspaceConfigurationDictionary` which is a dictionary of workspace names and their configurations. It provides `Configure<T>` and `ConfigureDefault` methods to configure the default workspace and also configure isolated workspaces by using the workspace type.

- Configure method passes `WorkspaceConfiguration` object to the configure action. You can configure the `ChatClient` and `Kernel` by using the `ConfigureChatClient` and `ConfigureKernel` methods.

- Both **ChatClient** and **Kernel** have a `Builder` property and `BuilderConfigurers` property. 
  - `Builder` is set once and is used to build the `ChatClient` or `Kernel` instance.
  - `BuilderConfigurers` is a list of actions that are applied to the `Builder` instance for incremental changes.These actions are executed in the order they are added.


### Microsoft.Extensions.AI
To configure a chat client, you'll need a LLM provider package such as [Microsoft.Extensions.AI.OpenAI](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI) or [OllamaSharp](https://www.nuget.org/packages/OllamaSharp/) to configure a chat client.

_The following example is requires [OllamaSharp](https://www.nuget.org/packages/OllamaSharp/) package to be installed._


Demonstration of the default workspace configuration:
```csharp
[DependsOn(typeof(AbpAIModule))]
public class MyProjectModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAIWorkspaceOptions>(options =>
        {
            options.Workspaces.ConfigureDefault(configuration =>
            {
                configuration.ConfigureChatClient(chatClientConfiguration =>
                {
                    chatClientConfiguration.Builder = new ChatClientBuilder(
                        sp => new OllamaApiClient("http://localhost:11434", "mistral")
                    );
                });
            });
        });
    }
}
```


Demonstration of the isolated workspace configuration:
```csharp
[DependsOn(typeof(AbpAIModule))]
public class MyProjectModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAIWorkspaceOptions>(options =>
        {
            options.Workspaces.Configure<CommentSummarization>(configuration =>
            {
                configuration.ConfigureChatClient(chatClientConfiguration =>
                {
                    chatClientConfiguration.Builder = new ChatClientBuilder(
                        sp => new OllamaApiClient("http://localhost:11434", "mistral")
                    );
                });
            });
        });
    }
}
```

### Semantic Kernel
To configure a kernel, you'll need a kernel connector package such as [Microsoft.SemanticKernel.Connectors.OpenAI](Microsoft.SemanticKernel.Connectors.OpenAI) to configure a kernel to use a specific LLM provider.

_The following example is requires [Microsoft.SemanticKernel.Connectors.AzureOpenAI](Microsoft.SemanticKernel.Connectors.AzureOpenAI) package to be installed._

Demonstration of the default workspace configuration:
```csharp
[DependsOn(typeof(AbpAIModule))]
public class MyProjectModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAIOptions>(options =>
        {
            options.Workspaces.ConfigureDefault(configuration =>
            {
                configuration.ConfigureKernel(kernelConfiguration =>
                {
                    kernelConfiguration.Builder = Kernel.CreateBuilder()
                        .AddAzureOpenAIChatClient("...", "...");
                });
                // Note: Chat client is not configured here
            });
        });
    }
}
```

Demonstration of the isolated workspace configuration:
```csharp
[DependsOn(typeof(AbpAIModule))]
public class MyProjectModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAIOptions>(options =>
        {
            options.Workspaces.Configure<CommentSummarization>(configuration =>
            {
                configuration.ConfigureKernel(kernelConfiguration =>
                {
                    kernelConfiguration.Builder = Kernel.CreateBuilder()
                        .AddAzureOpenAIChatClient("...", "...");
                });
            });
        });
    }
}
```