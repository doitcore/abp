# Elsa Module (Pro)

> You must have an ABP Team or a higher license to use this module.

This module is used integrate [Elsa Workflows](https://docs.elsaworkflows.io/) into ABP Framework applications. 

## How to install

Elsa module is not installed in [the startup templates](../solution-templates/layered-web-application). So, it needs to be installed manually. There are two ways of installing a module into your application.

### Using ABP CLI

ABP CLI allows adding a module to a solution using ```add-module``` command. You can check its [documentation](../cli#add-module) for more information. So, elsa module can be added using the command below;

```bash
abp add-module Volo.Elsa
```

### Manual Installation

If you modified your solution structure, adding module using ABP CLI might not work for you. In such cases,  elsa module can be added to a solution manually.

In order to do that, add packages listed below to matching project on your solution. For example, `Volo.Abp.Elsa.Application` package to your **{ProjectName}.Application.csproj** like below;

```json
<PackageReference Include="Volo.Abp.Elsa.Application" Version="x.x.x" />
```

After adding the package reference, open the module class of the project (eg: `{ProjectName}ApplicationModule`) and add the below code to the `DependsOn` attribute.

```csharp
[DependsOn(
  //...
  typeof(AbpElsaApplicationModule)
)]
```

> If you are using Blazor Web App, you need to add the `Volo.Elsa.Admin.Blazor.WebAssembly` package to the **{ProjectName}.Blazor.Client.csproj** project and ad the `Volo.Elsa.Admin.Blazor.Server` package to the **{ProjectName}.Blazor.csproj** project.

## The Elsa Module

The Elsa Workflows have own database provider, also have Tenant/Role/User system, they are under active development, So, the ABP Elsa module is not fully integrated yet. Below is the current status of each module in the ABP Elsa module.

- `AbpElsaAspNetCoreModule(Volo.Elsa.Abp.AspNetCore)` module is used to integrate Elsa authentication.
- `AbpElsaIdentityModule(Volo.Elsa.Abp.Identity)` module is used to integrate ABP Identity authentication.
- `AbpElsaApplicationModule(Volo.Elsa.Abp.Application)` and `AbpElsaApplicationContractsModule(Volo.Elsa.Abp.Application.Contracts)` modules are used to define the Elsa permissions.

The rest of the projects/modules are basically empty and will be implemented in the future based on the Elsa features.

- `AbpElsaDomainModule(Volo.Elsa.Abp.Domain)`
- `AbpElsaEntityFrameworkCoreModule(Volo.Elsa.Abp.EntityFrameworkCore)`
- `AbpElsaHttpApiModule(Volo.Elsa.Abp.HttpApi)`
- `AbpElsaHttpApiClientModule(Volo.Elsa.Abp.HttpApi.Client)`
- `AbpElsaBlazorModule(Volo.Elsa.Abp.Blazor)`
- `AbpElsaBlazorServerModule(Volo.Elsa.Abp.Blazor.Server)`
- `AbpElsaBlazorWebAssemblyModule(Volo.Elsa.Abp.Blazor.WebAssembly)`
- `AbpElsaWebModule(Volo.Elsa.Abp.Web)`

### Elsa Module Permissions

The Elsa workflow API points will check the permissions, It also have a `*` wildcard permission to allow all permissions.

ABP Elsa module defines all permissions that are used in the Elsa workflow, You can use ABP Permission Management module to manage the permissions.

`AbpElsaAspNetCoreModule(Volo.Elsa.Abp.AspNetCore)` module will check and add these permissions to current user claims.

![Elsa Permissions](../images/elsa-permissions.png)

You can also grant parts of the permissions to a role or user. It will add the `permissions` claims to the current user's `Cookies` or `Token`. Elsa Server will read the claims and allow or deny the access.

![Elsa Part Permissions](../images/elsa-part-permissions.png)

### Elsa Studio

Elsa Studio is a **independent** web application that allows you to design, manage, and execute workflows. It is built using **Blazor Server/WebAssembly**.

Elsa Studio requires authentication to access it. ABP Elsa module provides two ways to authenticate Elsa Studio.

### Elsa Studio Password Flow Authentication

The `AbpElsaIdentityModule(Volo.Elsa.Abp.Identity)` module is used to integrate [ABP Identity module](https://abp.io/docs/commercial/latest/modules/identity) to check Elsa Studio username and password against ABP Identity. 

You need to replace `UseIdentity` with `UseAbpIdentity` when configuring Elsa in your Elsa server project.

```csharp
context.Services
    .AddElsa(elsa => elsa
        .UseAbpIdentity(identity =>
        {
            identity.TokenOptions = options => options.SigningKey = "large-signing-key-for-signing-JWT-tokens";
        });
    );
```

After that, you can use add below code use `Identity` as login method in your Elsa Studio client project.

```csharp
builder.Services.AddLoginModule().UseElsaIdentity();
```

![elsa-login](../images/elsa-password-login.png)

![elsa-main](../images/elsa-main-page.png)

### Elsa Studio Code Flow Authentication

Abp applications uses OpenIddict for authentication. So, you can use the [Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/) to authenticate Elsa Studio.

Add code below to your Elsa Studio client project.

```csharp
builder.Services.AddLoginModule().UseOpenIdConnect(connectConfiguration =>
{
    var authority = configuration["AuthServer:Authority"]!.TrimEnd('/'); // Your Server URL
    connectConfiguration.AuthEndpoint = $"{authority}/connect/authorize";
    connectConfiguration.TokenEndpoint = $"{authority}/connect/token";
    connectConfiguration.EndSessionEndpoint = $"{authority}/connect/endsession";
    connectConfiguration.ClientId = configuration["AuthServer:ClientId"]!;
    connectConfiguration.Scopes = ["openid", "profile", "email", "phone", "roles", "offline_access", "ElsaDemoAppServer"];
});
```

After that, Elsa Studio will redirect to your ABP application login page, then redirect back to Elsa Studio after successful login.

### Elsa Module Demo Apps

You can get the Elsa demo application from by downloading the ABP Elsa module source code. It is located in the `app` folder.

- `ElsaDemoApp.Server` is an ABP application with Identity and Elsa modules. It is used as the authentication server and Elsa workflow server.
- `ElsaDemoApp.Studio.WASM` is a Blazor WebAssembly application with Elsa Studio. It is used as the Elsa Studio client application.
- `ElsaDemoApp.Ordering` and `ElsaDemoApp.Payment` are two microservices that can be used to test the Elsa workflows in distributed systems.

![Elsa Module Structure](../images/elsa-module-structure.png)


#### Running the Demo Application

The `ElsaDemoApp.Server` has a pre-defined Elsa workflow that creates an order and processes the payment using Elsa workflows, and use ABP distributed event bus to coordinate the workflow.

```cs
public class OrderWorkflow : WorkflowBase
{
    public const string Name = "OrderWorkflow";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(Name);
        builder.Root = new Sequence
        {
            Activities =
            {
                // Will publish NewOrderEto event to the Ordering microservice,  Ordering microservice will create the order and publish OrderPlaced event
                new CreateOrderActivity(),

                // Wait for the OrderPlaced event, This event is triggered by the Ordering microservice, and Elsa will make workflow continue to the next activity
                new OrderPlacedEvent(),

                // This activity will publish RequestPaymentEto event to the Payment microservice, Payment microservice will process the payment and publish PaymentCompleted event
                new RequestPaymentActivity(),

                //  Wait for the PaymentCompleted event, This event is triggered by the Payment microservice, and Elsa will make workflow continue to the next activity
                new PaymentCompletedEvent(),

                // This activity will send an email to the customer indicating that the payment is completed
                new PaymentCompletedActivity()
            }
        };
    }
}
```

Please follow the steps below to run the demo application.

> The demo application uses SQL Server LocalDB as the database provider and Redis and RabbitMQ, Please make sure you have them installed and running on your machine.

1. Change the connection string in all `appsettings.json` file if needed. The default connection string uses a local SQL Server instance.
2. Run `ElsaDemoApp.Server` project to migrate the database(`dotnet run --migrate-database`) and start the server.
3. Run `ElsaDemoApp.Studio.WASM` project to start the Elsa Studio client application.
4. Run `ElsaDemoApp.Ordering` project to start the Ordering microservice.
5. Run `ElsaDemoApp.Payment` project to start the Payment microservice.

You can login into `ElsaDemoApp.Server` application and navigate to the `https://localhost:5001/Ordering` page to create an order. 

![Create Order](../images/elsa-create-order.png)

After that, you can navigate to the `ElsaDemoApp.Studio.WASM` application and see the workflow instance created, running and completed.

![Workflow Instances](../images/elsa-workflow-instances.png)
