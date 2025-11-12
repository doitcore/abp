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
Both `Microsoft.Extensions.AI` and `Microsoft.SemanticKernel` are supported. Microsoft provides `IChatClient` interface to cover multiple chat client integrations. ABP respects it and uses it by default. You'll be use this interface to access the chat client from your services. In the other hand, `Microsoft.SemanticKernel` provides `Kernel` object to configure and execute AI capabilities ABP respects it and uses it by default but the Kernel is not directly accessible from your services. You'll be use `IKernelAccessor` service to access the `Kernel` object from your services.

You can use both of them in your application by resolving `IChatClient` or `IKernelAccessor` services from the [service provider](../fundamentals/dependency-injection.md). `IChatClient` is the original interface from `Microsoft.Extensions.AI` but `IKernelAccessor` is a custom service that is used to access the `Kernel` object from `Microsoft.SemanticKernel`.

Check the following documentation for the usage of `Microsoft.Extensions.AI` and `Microsoft.SemanticKernel`:

- [Usage of Microsoft.Extensions.AI](./microsoft-extensions-ai.md)
- [Usage of Microsoft.SemanticKernel](./microsoft-semantic-kernel.md)
