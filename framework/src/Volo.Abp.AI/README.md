### Volo.Abp.AI for application developers

Use this package to configure and consume two AI stacks in ABP apps with a shared “workspace” scope:
- Microsoft.Extensions.AI Chat Clients
- Microsoft.SemanticKernel Kernels

Key ideas:
- Decorate a class with `WorkspaceNameAttribute` to define a workspace. The same workspace name is used for both Chat Client and Kernel.
- Resolve services either by workspace type (`IChatClient<TWorkSpace>`, `IKernel<TWorkSpace>`) or as defaults (`IChatClient`, `Kernel`).

### 1) Add module dependency

```csharp
using Volo.Abp.AI;
using Volo.Abp.Modularity;

[DependsOn(typeof(AbpAIModule))]
public class YourAppModule : AbpModule
{
}
```

### 2) Define a workspace

```csharp
using Volo.Abp.AI;

[WorkspaceName("CommentSummarizer")]
public class CommentSummarizer { }
```

If you omit the attribute, the type’s full name is used as the workspace name.

### 3) Configure providers per workspace

Configure in your module (in `ConfigureServices` using `PreConfigure<AbpAIOptions>`). You can set defaults and/or configure specific workspaces.

```csharp
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Volo.Abp.AI;
using Volo.Abp.Modularity;

public class YourAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAIOptions>(options =>
        {
            // Default ChatClient (inject IChatClient)
            options.ChatClients.ConfigureDefault(cfg =>
            {
                cfg.Builder = new ChatClientBuilder();
                cfg.ConfigureBuilder(b =>
                {
                    // Example: OpenAI provider (Microsoft.Extensions.AI.OpenAI)
                    b.UseOpenAIChatClient("gpt-4o-mini", apiKey: "<OPENAI_API_KEY>");
                });
            });

            // Default Kernel (inject Kernel)
            options.Kernels.ConfigureDefault(cfg =>
            {
                var kb = Kernel.CreateBuilder();
                // Example: OpenAI connector (Microsoft.SemanticKernel.Connectors.OpenAI)
                kb.AddOpenAIChatCompletion("gpt-4o-mini", "<OPENAI_API_KEY>");
                cfg.Builder = kb;
            });

            // Workspace-scoped ChatClient (inject IChatClient<CommentSummarizer>)
            options.ChatClients.Configure<CommentSummarizer>(cfg =>
            {
                cfg.Builder = new ChatClientBuilder();
                cfg.ConfigureBuilder(b => b.UseOpenAIChatClient("gpt-4o-mini", "<OPENAI_API_KEY>"));
            });

            // Workspace-scoped Kernel (inject IKernel<CommentSummarizer>)
            options.Kernels.Configure<CommentSummarizer>(cfg =>
            {
                var kb = Kernel.CreateBuilder();
                kb.AddOpenAIChatCompletion("gpt-4o-mini", "<OPENAI_API_KEY>");
                cfg.Builder = kb;
            });
        });
    }
}
```

Notes:
- `cfg.Builder` is required for both Chat Client and Kernel.
- You can call `cfg.ConfigureBuilder(...)` multiple times; actions run in order.

### 4) Resolve and use services

Defaults (from `ConfigureDefault`):

```csharp
public class MyService
{
    private readonly IChatClient _chatClient; // Microsoft.Extensions.AI
    private readonly Kernel _kernel;          // Microsoft.SemanticKernel

    public MyService(IChatClient chatClient, Kernel kernel)
    {
        _chatClient = chatClient;
        _kernel = kernel;
    }
}
```

Workspace-scoped (typed):

```csharp
public class CommentSummarizerService
{
    private readonly IChatClient<CommentSummarizer> _chatClient;
    private readonly IKernel<CommentSummarizer> _kernel;

    public CommentSummarizerService(
        IChatClient<CommentSummarizer> chatClient,
        IKernel<CommentSummarizer> kernel)
    {
        _chatClient = chatClient;
        _kernel = kernel;
    }
}
```

Access the original Semantic Kernel instance via `IKernel<TWorkSpace>.Kernel`:

```csharp
public class KernelUsage
{
    private readonly IKernel<CommentSummarizer> _workspaceKernel;

    public KernelUsage(IKernel<CommentSummarizer> workspaceKernel)
    {
        _workspaceKernel = workspaceKernel;
    }

    public async Task RunAsync()
    {
        var sk = _workspaceKernel.Kernel; // Microsoft.SemanticKernel.Kernel
        // Use SK APIs directly
    }
}
```

### Frequently used variations

- Only Chat Client per workspace: configure `options.ChatClients.Configure<YourWorkspace>(...)`.
- Only Kernel per workspace: configure `options.Kernels.Configure<YourWorkspace>(...)`.
- Single global setup: just use `ConfigureDefault` for either or both; inject the default services.

### Terminology

- `WorkspaceNameAttribute`: names a workspace; used for both stacks.
- `IChatClient<TWorkSpace>` and `IKernel<TWorkSpace>`: typed services bound to a workspace.
- `IChatClient` and `Kernel`: defaults if configured via `ConfigureDefault`.
