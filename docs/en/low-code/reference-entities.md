```json
//[doc-seo]
{
    "Description": "Link dynamic entities to existing C# entities like IdentityUser using Reference Entities in the ABP Low-Code System."
}
```

# Reference Entities

Reference Entities allow you to create foreign key relationships from dynamic entities to **existing (static) C# entities** that are already defined in your application or in ABP modules.

## Overview

Dynamic entities defined via [Attributes](fluent-api.md) or [model.json](model-json.md) normally reference other dynamic entities. However, you may need to link to entities like ABP's `IdentityUser`, `Tenant`, or your own C# entity classes. Reference entities make this possible.

Unlike dynamic entities, reference entities are **read-only** from the Low-Code System's perspective — they don't get CRUD pages or APIs. They are used solely for:

* **Foreign key lookups** — dropdown selection in UI forms
* **Display values** — showing the referenced entity's display property in grids
* **Query support** — querying via the [Scripting API](scripting-api.md)

## Registering Reference Entities

Register reference entities in your Domain module's `ConfigureServices` using `AbpDynamicEntityConfig.ReferencedEntityList`:

````csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    AbpDynamicEntityConfig.ReferencedEntityList.Add<IdentityUser>(
        "UserName"        // Default display property
    );

    AbpDynamicEntityConfig.ReferencedEntityList.Add<IdentityUser>(
        "UserName",       // Default display property
        "UserName",       // Exposed properties
        "Email",
        "PhoneNumber"
    );
}
````

### `Add<TEntity>` Method

````csharp
public void Add<TEntity>(
    string defaultDisplayProperty,
    params string[] properties
) where TEntity : class, IEntity<Guid>
````

| Parameter | Description |
|-----------|-------------|
| `defaultDisplayProperty` | Property name used as display value in lookups |
| `properties` | Additional properties to expose (optional) |

> The entity type must implement `IEntity<Guid>`.

## Using Reference Entities in model.json

Reference a registered entity in a foreign key definition:

```json
{
  "name": "UserId",
  "foreignKey": {
    "entityName": "Volo.Abp.Identity.IdentityUser"
  }
}
```

The entity name must match the CLR type's full name. The module automatically detects that this is a reference entity and uses the registered `ReferenceEntityDescriptor`.

## Using Reference Entities with Attributes

Use the `[DynamicForeignKey]` attribute on a Guid property:

````csharp
[DynamicEntity]
public class Customer
{
    [DynamicForeignKey("Volo.Abp.Identity.IdentityUser", "UserName")]
    public Guid? UserId { get; set; }
}
````

## How It Works

The `ReferenceEntityDescriptor` class stores metadata about the reference entity:

* `Name` — Full CLR type name
* `Type` — The actual CLR type
* `DefaultDisplayPropertyName` — Display property for lookups
* `Properties` — List of `ReferenceEntityPropertyDescriptor` entries

When a foreign key points to a reference entity, the `ForeignKeyDescriptor` populates its `ReferencedEntityDescriptor` and `ReferencedDisplayPropertyDescriptor` instead of the standard `EntityDescriptor` fields.

## Querying Reference Entities in Scripts

Reference entities can be queried via the [Scripting API](scripting-api.md):

```javascript
// Query reference entity in interceptor or custom endpoint
var user = await db.get('Volo.Abp.Identity.IdentityUser', userId);
if (user) {
    context.log('User: ' + user.UserName);
}
```

## Limitations

* **Read-only**: Reference entities do not get CRUD operations, permissions, or UI pages.
* **No child entities**: You cannot define a reference entity as a parent in parent-child relationships.
* **Guid keys only**: Reference entities must have `Guid` primary keys (`IEntity<Guid>`).
* **Explicit registration required**: Each reference entity must be registered in code before use.

## Common Reference Entities

| Entity | Name for `entityName` | Typical Display Property |
|--------|----------------------|--------------------------|
| ABP Identity User | `Volo.Abp.Identity.IdentityUser` | `UserName` |

## See Also

* [model.json Structure](model-json.md)
* [Foreign Access](foreign-access.md)
* [Fluent API & Attributes](fluent-api.md)
