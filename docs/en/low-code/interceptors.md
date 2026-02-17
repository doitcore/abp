```json
//[doc-seo]
{
    "Description": "Add custom business logic to dynamic entity CRUD operations using Interceptors in the ABP Low-Code Module. Validate, transform, and react to data changes with JavaScript."
}
```

# Interceptors

Interceptors allow you to run custom JavaScript code before, after, or instead of Create, Update, and Delete operations on dynamic entities.

## Interceptor Types

| Command | Type | When Executed |
|---------|------|---------------|
| `Create` | `Pre` | Before entity creation — validation, default values |
| `Create` | `Post` | After entity creation — notifications, related data |
| `Create` | `Replace` | Instead of entity creation — the default DB insert is skipped, only JavaScript runs |
| `Update` | `Pre` | Before entity update — validation, authorization |
| `Update` | `Post` | After entity update — sync, notifications |
| `Update` | `Replace` | Instead of entity update — the default DB update is skipped, only JavaScript runs |
| `Delete` | `Pre` | Before entity deletion — dependency checks |
| `Delete` | `Post` | After entity deletion — cleanup |
| `Delete` | `Replace` | Instead of entity deletion — the default DB delete is skipped, only JavaScript runs |

## Defining Interceptors with Attributes

Use the `[DynamicEntityCommandInterceptor]` attribute on a C# class:

````csharp
[DynamicEntity]
[DynamicEntityCommandInterceptor(
    "Create",
    InterceptorType.Pre,
    "if(!context.commandArgs.data['Name']) { globalError = 'Name is required!'; }"
)]
[DynamicEntityCommandInterceptor(
    "Create",
    InterceptorType.Post,
    "context.log('Entity created: ' + context.commandArgs.entityId);"
)]
public class Organization
{
    public string Name { get; set; }
}
````

The `Name` parameter must be one of: `"Create"`, `"Update"`, or `"Delete"`. The `InterceptorType` can be `Pre`, `Post`, or `Replace`. When `Replace` is used, the default database operation is completely skipped and only your JavaScript handler executes. Multiple interceptors can be added to the same class (`AllowMultiple = true`).

## Defining Interceptors in model.json

Add interceptors to the `interceptors` array of an entity:

```json
{
  "name": "LowCodeDemo.Customers.Customer",
  "interceptors": [
    {
      "commandName": "Create",
      "type": "Pre",
      "javascript": "if(context.commandArgs.data['Name'] == 'Invalid') {\n  globalError = 'Invalid Customer Name!';\n}"
    }
  ]
}
```

### Interceptor Descriptor

| Field | Type | Description |
|-------|------|-------------|
| `commandName` | string | `"Create"`, `"Update"`, or `"Delete"` |
| `type` | string | `"Pre"`, `"Post"`, or `"Replace"` |
| `javascript` | string | JavaScript code to execute |

## JavaScript Context

Inside interceptor scripts, you have access to:

### `context.commandArgs`

| Property | Type | Description |
|----------|------|-------------|
| `data` | object | Entity data dictionary (for Create/Update) |
| `entityId` | string | Entity ID (for Update/Delete) |
| `getValue(name)` | function | Get a property value |
| `setValue(name, value)` | function | Set a property value (Pre-interceptors only) |

### `context.currentUser`

| Property | Type | Description |
|----------|------|-------------|
| `isAuthenticated` | bool | Whether user is logged in |
| `userName` | string | Username |
| `email` | string | Email address |
| `roles` | string[] | User's role names |
| `id` | string | User ID |

### `context.emailSender`

| Method | Description |
|--------|-------------|
| `sendAsync(to, subject, body)` | Send an email |

### `context.log(message)`

Log a message (use instead of `console.log`).

### `db` (Database API)

Full access to the [Scripting API](scripting-api.md) for querying and mutating data.

### `globalError`

Set this variable to a string to **abort** the operation and return an error:

```javascript
globalError = 'Cannot delete this entity!';
```

## Examples

### Pre-Create: Validation

```json
{
  "commandName": "Create",
  "type": "Pre",
  "javascript": "if(!context.commandArgs.data['Name']) {\n  globalError = 'Organization name is required!';\n}"
}
```

### Post-Create: Email Notification

```json
{
  "commandName": "Create",
  "type": "Post",
  "javascript": "if(context.currentUser.isAuthenticated && context.emailSender) {\n  await context.emailSender.sendAsync(\n    context.currentUser.email,\n    'New Order Created',\n    'Order total: $' + context.commandArgs.data['TotalAmount']\n  );\n}"
}
```

### Pre-Update: Role-Based Authorization

```json
{
  "commandName": "Update",
  "type": "Pre",
  "javascript": "if(context.commandArgs.data['IsDelivered']) {\n  if(!context.currentUser.roles.includes('admin')) {\n    globalError = 'Only administrators can mark orders as delivered!';\n  }\n}"
}
```

### Pre-Delete: Business Rule Check

```json
{
  "commandName": "Delete",
  "type": "Pre",
  "javascript": "var project = await db.get('LowCodeDemo.Projects.Project', context.commandArgs.entityId);\nif(project.Budget > 100000) {\n  globalError = 'Cannot delete high-budget projects!';\n}"
}
```

### Pre-Update: Negative Value Check

```json
{
  "commandName": "Update",
  "type": "Pre",
  "javascript": "if(context.commandArgs.data['Quantity'] < 0) {\n  globalError = 'Quantity cannot be negative!';\n}"
}
```

### Replace-Create: Custom Insert Logic

When you need to completely replace the default create operation with custom logic:

```json
{
  "commandName": "Create",
  "type": "Replace",
  "javascript": "var data = context.commandArgs.data;\ndata['Code'] = 'PRD-' + Date.now();\nawait db.insert('LowCodeDemo.Products.Product', data);\ncontext.log('Product created with custom code: ' + data['Code']);"
}
```

> When `Replace` is used, the standard database operation does not run. You are responsible for performing any necessary persistence in your JavaScript handler.

### Pre-Update: Self-Reference Check

```json
{
  "commandName": "Update",
  "type": "Pre",
  "javascript": "if(context.commandArgs.data.ParentCategoryId === context.commandArgs.entityId) {\n  globalError = 'A category cannot be its own parent!';\n}"
}
```

## See Also

* [Scripting API](scripting-api.md)
* [model.json Structure](model-json.md)
* [Custom Endpoints](custom-endpoints.md)
