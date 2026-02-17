```json
//[doc-seo]
{
    "Description": "Define dynamic entities using C# attributes and configure them with the Fluent API in the ABP Low-Code System. The primary way to build auto-generated admin panels."
}
```

# Fluent API & Attributes

C# Attributes and the Fluent API are the **recommended way** to define dynamic entities. They provide compile-time checking, IntelliSense, refactoring support, and keep your entity definitions close to your domain code.

## Quick Start

### Step 1: Define an Entity

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

    public DateTime? ReleaseDate { get; set; }
}
````

### Step 2: Register the Assembly

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    AbpDynamicEntityConfig.SourceAssemblies.Add(
        new DynamicEntityAssemblyInfo(typeof(YourDomainModule).Assembly)
    );
}
````

### Step 3: Add Migration and Run

```bash
dotnet ef migrations add Added_Product
dotnet ef database update
```

You now have a complete Product management page with data grid, create/edit modals, search, sorting, and pagination.

### Step 4: Add Relationships

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

The `Order` page now has a foreign key dropdown for Customer, and `OrderLine` is managed as a nested child inside the Order detail modal.

## Three-Layer Configuration System

The Low-Code System uses a layered configuration model. From lowest to highest priority:

1. **Code Layer** — C# classes with `[DynamicEntity]` and other attributes
2. **JSON Layer** — `model.json` file (see [model.json Structure](model-json.md))
3. **Fluent Layer** — `AbpDynamicEntityConfig.EntityConfigurations`

A `DefaultsLayer` runs last to fill in any missing values with conventions.

> When the same entity or property is configured in multiple layers, the higher-priority layer wins.

## C# Attributes Reference

### `[DynamicEntity]`

Marks a class as a dynamic entity. The entity name is derived from the class namespace and name.

````csharp
[DynamicEntity]
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
````

Use the `Parent` property for parent-child (master-detail) relationships:

````csharp
[DynamicEntity(Parent = "MyApp.Orders.Order")]
public class OrderLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
````

### `[DynamicEntityUI]`

Configures entity-level UI. Entities with `PageTitle` get a menu item and a dedicated page:

````csharp
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Product Management")]
public class Product
{
    // ...
}
````

### `[DynamicForeignKey]`

Defines a foreign key relationship on a `Guid` property:

````csharp
[DynamicForeignKey("MyApp.Customers.Customer", "Name", ForeignAccess.Edit)]
public Guid CustomerId { get; set; }
````

| Parameter | Description |
|-----------|-------------|
| `entityName` | Full name of the referenced entity (or [reference entity](reference-entities.md)) |
| `displayPropertyName` | Property to show in lookups |
| `access` | `ForeignAccess.None`, `ForeignAccess.View`, or `ForeignAccess.Edit` (see [Foreign Access](foreign-access.md)) |

### `[DynamicPropertyUI]`

Controls property visibility and behavior in the UI:

````csharp
[DynamicPropertyUI(
    DisplayName = "Registration Number",
    IsAvailableOnListing = true,
    IsAvailableOnDataTableFiltering = true,
    CreationFormAvailability = EntityPropertyUIFormAvailability.Hidden,
    EditingFormAvailability = EntityPropertyUIFormAvailability.NotAvailable,
    QuickLookOrder = 100
)]
public string RegistrationNumber { get; set; }
````

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DisplayName` | string | null | Custom label for the property |
| `IsAvailableOnListing` | bool | `true` | Show in data grid |
| `IsAvailableOnDataTableFiltering` | bool | `true` | Show in filter panel |
| `CreationFormAvailability` | enum | `Available` | Visibility on create form |
| `EditingFormAvailability` | enum | `Available` | Visibility on edit form |
| `QuickLookOrder` | int | `-2` | Order in quick-look panel |

### `[DynamicPropertyServerOnly]`

Hides a property from API clients entirely. It is stored in the database but never returned to the client:

````csharp
[DynamicPropertyServerOnly]
public string InternalNotes { get; set; }
````

### `[DynamicPropertySetByClients]`

Controls whether clients can set this property value. Useful for computed or server-assigned fields:

````csharp
[DynamicPropertySetByClients(false)]
public string RegistrationNumber { get; set; }
````

### `[DynamicPropertyUnique]`

Marks a property as requiring unique values across all records:

````csharp
[DynamicPropertyUnique]
public string ProductCode { get; set; }
````

### `[DynamicEntityCommandInterceptor]`

Defines JavaScript interceptors on a class for CRUD lifecycle hooks:

````csharp
[DynamicEntity]
[DynamicEntityCommandInterceptor(
    "Create",
    InterceptorType.Pre,
    "if(!context.commandArgs.data['Name']) { globalError = 'Name is required!'; }"
)]
[DynamicEntityCommandInterceptor(
    "Delete",
    InterceptorType.Post,
    "context.log('Deleted: ' + context.commandArgs.entityId);"
)]
public class Organization
{
    public string Name { get; set; }
}
````

> The `Name` parameter must be one of: `"Create"`, `"Update"`, or `"Delete"`. The `InterceptorType` can be `Pre`, `Post`, or `Replace`. When `Replace` is used, the default DB operation is skipped entirely and only the JavaScript handler runs. **`Replace-Create` must return the new entity's Id** (e.g. `return result.Id;` after `db.insert`). Multiple interceptors can be added to the same class (`AllowMultiple = true`).

See [Interceptors](interceptors.md) for the full JavaScript context API.

### `[DynamicEnum]`

Marks an enum for use in dynamic entity properties:

````csharp
[DynamicEnum]
public enum OrganizationType
{
    Corporate = 0,
    Enterprise = 1,
    Startup = 2,
    Consulting = 3
}
````

Reference in an entity:

````csharp
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Organizations")]
public class Organization
{
    public string Name { get; set; }
    public OrganizationType OrganizationType { get; set; }
}
````

## Fluent API

The Fluent API has the **highest priority** in the configuration system. Use `AbpDynamicEntityConfig.EntityConfigurations` to override any attribute or JSON setting programmatically.

### Basic Usage

Configure in your Domain module's `ConfigureServices`:

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    AbpDynamicEntityConfig.EntityConfigurations.Configure(
        "MyApp.Products.Product",
        entity =>
        {
            entity.SetDisplayProperty("Name");

            entity.ConfigureProperty("Price", prop =>
            {
                prop.SetRequired(true);
                prop.SetUI(ui =>
                {
                    ui.SetDisplayName("Unit Price");
                    ui.SetCreationFormAvailability(EntityPropertyUIFormAvailability.Available);
                });
            });

            entity.ConfigureProperty("InternalNotes", prop =>
            {
                prop.SetServerOnly(true);
            });
        }
    );
}
````

### Entity Configuration Methods

| Method | Description |
|--------|-------------|
| `SetDisplayProperty(name)` | Set the display property for lookups |
| `SetParent(entityName)` | Set parent entity for nesting |
| `SetUI(action)` | Configure entity-level UI |
| `ConfigureProperty(name, action)` | Configure a specific property |
| `AddInterceptor(name, type, js)` | Add a JavaScript interceptor. `name`: `"Create"`, `"Update"`, or `"Delete"`. `type`: `Pre`, `Post`, or `Replace`. `Replace-Create` must return the new entity's Id |

### Property Configuration Methods

| Method | Description |
|--------|-------------|
| `SetRequired(bool)` | Mark as required |
| `SetUnique(bool)` | Mark as unique |
| `SetServerOnly(bool)` | Hide from clients |
| `SetAllowSetByClients(bool)` | Allow client writes |
| `SetForeignKey(entityName, displayProp, access)` | Configure foreign key |
| `SetUI(action)` | Configure property UI |

## Assembly Registration

Register assemblies containing `[DynamicEntity]` classes:

````csharp
AbpDynamicEntityConfig.SourceAssemblies.Add(
    new DynamicEntityAssemblyInfo(typeof(MyDomainModule).Assembly)
);
````

You can also register entity types directly:

````csharp
AbpDynamicEntityConfig.DynamicEntityTypes.Add(typeof(Product));
AbpDynamicEntityConfig.DynamicEnumTypes.Add(typeof(OrganizationType));
````

## Combining with model.json

Attributes and model.json work together seamlessly. A common pattern:

1. **Define core entities** with C# attributes (compile-time safety)
2. **Add additional entities** via model.json (no recompilation needed)
3. **Fine-tune configuration** with Fluent API (overrides everything)

The three-layer system merges all definitions:

```
Fluent API (highest) > JSON (model.json) > Code (Attributes) > Defaults (lowest)
```

For example, if an attribute sets `[DynamicPropertyUnique]` and model.json sets `"isUnique": false`, the JSON value wins because JSON layer has higher priority than Code layer.

## End-to-End Example

A complete e-commerce-style entity setup:

````csharp
// Enum
[DynamicEnum]
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3
}

// Customer entity
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Customers")]
public class Customer
{
    [DynamicPropertyUnique]
    public string Name { get; set; }

    [DynamicPropertyUI(DisplayName = "Phone Number", QuickLookOrder = 100)]
    public string Telephone { get; set; }

    [DynamicForeignKey("Volo.Abp.Identity.IdentityUser", "UserName")]
    public Guid? UserId { get; set; }

    [DynamicPropertyServerOnly]
    public string InternalNotes { get; set; }
}

// Product entity
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Products")]
public class Product
{
    [DynamicPropertyUnique]
    public string Name { get; set; }

    public decimal Price { get; set; }
    public int StockCount { get; set; }
}

// Order entity with child OrderLine
[DynamicEntity]
[DynamicEntityUI(PageTitle = "Orders")]
[DynamicEntityCommandInterceptor(
    "Update",
    InterceptorType.Pre,
    @"if(context.commandArgs.data['IsDelivered']) {
        if(!context.currentUser.roles.includes('admin')) {
            globalError = 'Only admins can mark as delivered!';
        }
    }"
)]
public class Order
{
    [DynamicForeignKey("MyApp.Customers.Customer", "Name", ForeignAccess.Edit)]
    public Guid CustomerId { get; set; }

    public decimal TotalAmount { get; set; }
    public bool IsDelivered { get; set; }
    public OrderStatus Status { get; set; }
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

Register everything in your Domain module:

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    AbpDynamicEntityConfig.SourceAssemblies.Add(
        new DynamicEntityAssemblyInfo(typeof(MyDomainModule).Assembly)
    );

    // Reference existing ABP entities
    AbpDynamicEntityConfig.ReferencedEntityList.Add<IdentityUser>("UserName");
}
````

This gives you four auto-generated pages (Customers, Products, Orders with nested OrderLines), complete with permissions, menu items, foreign key lookups, and interceptor-based business rules.

## See Also

* [model.json Structure](model-json.md)
* [Reference Entities](reference-entities.md)
* [Interceptors](interceptors.md)
