```json
//[doc-seo]
{
    "Description": "ABP Low-Code System: Build admin panels with auto-generated CRUD UI, APIs, and permissions using C# attributes and Fluent API. No boilerplate code needed."
}
```

# Low-Code System

The ABP Low-Code System lets you define entities using C# attributes or Fluent API and automatically generates:

* **Database tables** (via EF Core migrations)
* **CRUD REST APIs** (Get, GetList, Create, Update, Delete)
* **Permissions** (View, Create, Update, Delete per entity)
* **Menu items** (auto-added to the admin sidebar)
* **Full Blazor UI** (data grid, create/edit modals, filters, foreign key lookups)

No need to write entity classes, DTOs, application services, repositories, or UI pages manually.

## Why Low-Code?

Traditionally, adding a new entity to an ABP application requires:

1. Entity class in Domain
2. DbContext configuration in EF Core
3. DTOs in Application.Contracts
4. AppService in Application
5. Controller in HttpApi
6. Razor/Blazor pages in UI
7. Permissions, menu items, localization

**With Low-Code, a single C# class replaces all of the above:**

````csharp
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Products")]
public class Product
{
    [DynamicPropertyUnique]
    public string Name { get; set; }

    [DynamicPropertyUI(DisplayName = "Unit Price")]
    public decimal Price { get; set; }

    public int StockCount { get; set; }

    [DynamicForeignKey("MyApp.Categories.Category", "Name")]
    public Guid? CategoryId { get; set; }
}
````

Run `dotnet ef migrations add Added_Product` and start your application. You get a complete Product management page with search, filtering, sorting, pagination, create/edit forms, and foreign key dropdown — all auto-generated.

## Getting Started

### 1. Install NuGet Packages

| Package | Layer |
|---------|-------|
| `Volo.Abp.LowCode.Domain.Shared` | Domain.Shared |
| `Volo.Abp.LowCode.Domain` | Domain |
| `Volo.Abp.LowCode.Application.Contracts` | Application.Contracts |
| `Volo.Abp.LowCode.Application` | Application |
| `Volo.Abp.LowCode.HttpApi` | HttpApi |
| `Volo.Abp.LowCode.HttpApi.Client` | HttpApi.Client |
| `Volo.Abp.LowCode.EntityFrameworkCore` | EF Core |
| `Volo.Abp.LowCode.Blazor` | Blazor UI (SSR) |
| `Volo.Abp.LowCode.Blazor.Server` | Blazor Server |
| `Volo.Abp.LowCode.Blazor.WebAssembly` | Blazor WebAssembly |
| `Volo.Abp.LowCode.Installer` | Auto module discovery |

### 2. Add Module Dependencies

````csharp
[DependsOn(
    typeof(AbpLowCodeApplicationModule),
    typeof(AbpLowCodeEntityFrameworkCoreModule),
    typeof(AbpLowCodeHttpApiModule),
    typeof(AbpLowCodeBlazorModule)
)]
public class YourModule : AbpModule
{
}
````

### 3. Register Your Assembly

In your Domain module, register the assembly that contains your `[DynamicEntity]` classes:

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    AbpDynamicEntityConfig.SourceAssemblies.Add(
        new DynamicEntityAssemblyInfo(typeof(YourDomainModule).Assembly)
    );
}
````

### 4. Configure DbContext

Call `ConfigureDynamicEntities()` in your `DbContext`:

````csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ConfigureDynamicEntities();
}
````

### 5. Define Your First Entity

````csharp
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Customers")]
public class Customer
{
    public string Name { get; set; }

    [DynamicPropertyUI(DisplayName = "Phone Number")]
    public string Telephone { get; set; }

    [DynamicForeignKey("Volo.Abp.Identity.IdentityUser", "UserName")]
    public Guid? UserId { get; set; }
}
````

### 6. Add Migration and Run

```bash
dotnet ef migrations add Added_Customer
dotnet ef database update
```

Start your application — the Customer page is ready.

## Two Ways to Define Entities

### C# Attributes (Recommended)

Define entities as C# classes with attributes. You get compile-time checking, IntelliSense, and refactoring support:

````csharp
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Orders")]
public class Order
{
    [DynamicForeignKey("MyApp.Customers.Customer", "Name", ForeignAccess.Edit)]
    public Guid CustomerId { get; set; }

    public decimal TotalAmount { get; set; }
    public bool IsDelivered { get; set; }
}

[DynamicEntity(Parent = "MyApp.Orders.Order")]
public class OrderLine
{
    [DynamicForeignKey("MyApp.Products.Product", "Name")]
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
    public decimal Amount { get; set; }
}
````

See [Fluent API & Attributes](fluent-api.md) for the full attribute reference.

### model.json (Declarative)

Alternatively, define entities in a JSON file without writing C# classes:

```json
{
  "entities": [
    {
      "name": "MyApp.Customers.Customer",
      "displayProperty": "Name",
      "properties": [
        { "name": "Name", "isRequired": true },
        { "name": "Telephone", "ui": { "displayName": "Phone Number" } }
      ],
      "ui": { "pageTitle": "Customers" }
    }
  ]
}
```

See [model.json Structure](model-json.md) for the full specification.

> Both approaches can be combined. The [three-layer configuration system](fluent-api.md#configuration-priority) merges Attributes, JSON, and Fluent API with clear priority rules.

## Key Features

| Feature | Description | Documentation |
|---------|-------------|---------------|
| **Attributes & Fluent API** | Define entities with C# attributes and configure programmatically | [Fluent API & Attributes](fluent-api.md) |
| **model.json** | Declarative entity definitions in JSON | [model.json Structure](model-json.md) |
| **Reference Entities** | Link to existing entities like `IdentityUser` | [Reference Entities](reference-entities.md) |
| **Interceptors** | Pre/Post hooks for Create, Update, Delete with JavaScript | [Interceptors](interceptors.md) |
| **Scripting API** | Server-side JavaScript for database queries and CRUD | [Scripting API](scripting-api.md) |
| **Custom Endpoints** | REST APIs with JavaScript handlers | [Custom Endpoints](custom-endpoints.md) |
| **Foreign Access** | View/Edit related entities from the parent's UI | [Foreign Access](foreign-access.md) |

## Internals

### Domain Layer

* `DynamicModelManager`: Singleton managing all entity metadata with a layered configuration architecture (Code > JSON > Fluent > Defaults).
* `EntityDescriptor`: Entity definition with properties, foreign keys, interceptors, and UI configuration.
* `EntityPropertyDescriptor`: Property definition with type, validation, UI settings, and foreign key info.
* `IDynamicEntityRepository`: Repository for dynamic entity CRUD operations.

### Application Layer

* `DynamicEntityAppService`: CRUD operations for all dynamic entities.
* `DynamicEntityUIAppService`: UI definitions, menu items, and page configurations.
* `DynamicPermissionDefinitionProvider`: Auto-generates permissions per entity.
* `CustomEndpointExecutor`: Executes JavaScript-based custom endpoints.

### Database Providers

**Entity Framework Core**: Dynamic entities are configured as EF Core [shared-type entities](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=fluent-api#shared-type-entity-types) via the `ConfigureDynamicEntities()` extension method.

## See Also

* [Fluent API & Attributes](fluent-api.md)
* [model.json Structure](model-json.md)
* [Scripting API](scripting-api.md)
