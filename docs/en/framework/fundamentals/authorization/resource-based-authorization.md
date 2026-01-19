```json
//[doc-seo]
{
    "Description": "Learn how to implement resource-based authorization in ABP Framework for fine-grained access control on specific resource instances like documents, projects, or any entity."
}
```

# Resource-Based Authorization

**Resource-Based Authorization** is a powerful feature that enables fine-grained access control based on specific resource instances. While the standard [authorization system](./index.md) grants permissions at a general level (e.g., "can edit documents"), resource-based authorization allows you to grant permissions for a **specific** document, project, or any other entity rather than granting a permission for all of them.

## When to Use Resource-Based Authorization?

Consider resource-based authorization when you need to:

* Allow users to edit **only their own blog posts or documents**
* Grant access to **specific projects** based on team membership
* Implement document sharing **where different users have different access levels to the same document**
* Control access to resources based on ownership or custom sharing rules

**Example Scenarios:**

Imagine a document management system where:

- User A can view and edit Document 1
- User B can only view Document 1
- User A has no access to Document 2
- User C can manage permissions for Document 2

This level of granular control is what resource-based authorization provides.

## Usage

Implementing resource-based authorization involves three main steps:

1. **Define** resource permissions in your `PermissionDefinitionProvider`
2. **Check** permissions using `IResourcePermissionChecker`
3. **Manage** permissions via UI or using `IResourcePermissionManager` for programmatic usages

### Defining Resource Permissions

Define resource permissions in your `PermissionDefinitionProvider` class using the `AddResourcePermission` method:

```csharp
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Acme.BookStore.Permissions
{
    public class BookStorePermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup("BookStore");

            // Standard permissions
            myGroup.AddPermission("BookStore_Document_Create");
            
            // Permission to manage resource permissions (required)
            myGroup.AddPermission("BookStore_Document_ManagePermissions");

            // Resource-based permissions
            context.AddResourcePermission(
                name: "BookStore_Document_View",
                resourceName: "BookStore.Document",
                managementPermissionName: "BookStore_Document_ManagePermissions",
                displayName: LocalizableString.Create<BookStoreResource>("Permission:Document:View"),
                multiTenancySide: MultiTenancySides.Tenant
            );

            context.AddResourcePermission(
                name: "BookStore_Document_Edit",
                resourceName: "BookStore.Document",
                managementPermissionName: "BookStore_Document_ManagePermissions",
                displayName: LocalizableString.Create<BookStoreResource>("Permission:Document:Edit")
            );

            context.AddResourcePermission(
                name: "BookStore_Document_Delete",
                resourceName: "BookStore.Document",
                managementPermissionName: "BookStore_Document_ManagePermissions",
                displayName: LocalizableString.Create<BookStoreResource>("Permission:Document:Delete")
            );
        }
    }
}
```

The `AddResourcePermission` method requires the following parameters:

* `name`: A unique name for the resource permission.
* `resourceName`: An identifier for the resource type. This is typically the full name of the entity class (e.g., `BookStore.Document`).
* `managementPermissionName`: A standard permission that controls who can manage resource permissions. Users with this permission can grant/revoke resource permissions for specific resources.
* `displayName`: (Optional) A localized display name shown in the UI.
* `multiTenancySide`: (Optional) Specifies on which side of a multi-tenant application this permission can be used. Accepts `MultiTenancySides.Host` (only for the host side), `MultiTenancySides.Tenant` (only for tenants), or `MultiTenancySides.Both` (default, available on both sides). 

### Checking Resource Permissions

Use the `IResourcePermissionChecker` service to check if a user/role/client has a specific permission for a resource:

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions.Resources;

namespace Acme.BookStore.Documents
{
    public class DocumentAppService : ApplicationService, IDocumentAppService
    {
        private readonly IResourcePermissionChecker _resourcePermissionChecker;
        private readonly IDocumentRepository _documentRepository;

        public DocumentAppService(
            IResourcePermissionChecker resourcePermissionChecker,
            IDocumentRepository documentRepository)
        {
            _resourcePermissionChecker = resourcePermissionChecker;
            _documentRepository = documentRepository;
        }

        public async Task<DocumentDto> GetAsync(Guid id)
        {
            // Check if the current user can view this specific document
            if (!await _resourcePermissionChecker.IsGrantedAsync(
                "BookStore_Document_View",
                "BookStore.Document",
                id.ToString()))
            {
                throw new AbpAuthorizationException(
                    "You don't have permission to view this document.");
            }

            var document = await _documentRepository.GetAsync(id);
            return ObjectMapper.Map<Document, DocumentDto>(document);
        }

        public async Task UpdateAsync(Guid id, UpdateDocumentDto input)
        {
            // Check if the current user can edit this specific document
            if (!await _resourcePermissionChecker.IsGrantedAsync(
                "BookStore_Document_Edit",
                "BookStore.Document",
                id.ToString()))
            {
                throw new AbpAuthorizationException(
                    "You don't have permission to edit this document.");
            }

            var document = await _documentRepository.GetAsync(id);
            document.Title = input.Title;
            document.Content = input.Content;
            await _documentRepository.UpdateAsync(document);
        }
    }
}
```

In this example, the `DocumentAppService` injects `IResourcePermissionChecker` and uses its `IsGrantedAsync` method to verify if the current user has the required permission for a specific document before performing the operation. The method takes the permission name, resource name, and the resource key (document ID) as parameters.

#### Using with Entities (IKeyedObject)

ABP entities implement the `IKeyedObject` interface, which provides a `GetObjectKey()` method. For entities with a primary key, `GetObjectKey()` returns the key as a string. This enables convenient extension methods on `IResourcePermissionChecker`:

```csharp
// Instead of this:
await _resourcePermissionChecker.IsGrantedAsync(
    "BookStore_Document_View",
    "BookStore.Document",
    document.Id.ToString());

// You can write:
await _resourcePermissionChecker.IsGrantedAsync(
    "BookStore_Document_View",
    document);
```

> See the [Entities documentation](../../architecture/domain-driven-design/entities.md) for more information about the `IKeyedObject` interface.

#### Checking Multiple Permissions

You can check multiple permissions at once using the overload that accepts an array of permission names:

```csharp
public async Task<DocumentPermissionsDto> GetPermissionsAsync(Guid id)
{
    var result = await _resourcePermissionChecker.IsGrantedAsync(
        new[] { 
            "BookStore_Document_View", 
            "BookStore_Document_Edit", 
            "BookStore_Document_Delete" 
        },
        "BookStore.Document",
        id.ToString());

    return new DocumentPermissionsDto
    {
        CanView = result.Result["BookStore_Document_View"] == PermissionGrantResult.Granted,
        CanEdit = result.Result["BookStore_Document_Edit"] == PermissionGrantResult.Granted,
        CanDelete = result.Result["BookStore_Document_Delete"] == PermissionGrantResult.Granted
    };
}
```

### Managing Resource Permissions

Once you have defined resource permissions, you need a way to grant or revoke them for specific users, roles, or clients. The [Permission Management Module](../../../modules/permission-management.md) provides the infrastructure for managing resource permissions:

- **UI Components**: Built-in modal dialogs for managing resource permissions on all supported UI frameworks (MVC/Razor Pages, Blazor, and Angular). These components allow administrators to grant or revoke permissions for users and roles on specific resource instances through a user-friendly interface.
- **`IResourcePermissionManager` Service**: A service for programmatically granting, revoking, and querying resource permissions at runtime. This is useful for scenarios like automatically granting permissions when a resource is created, implementing sharing functionality, or integrating with external systems.

> See the [Permission Management Module](../../../modules/permission-management.md#resource-permission-management-dialog) documentation for detailed information on using the UI components and the `IResourcePermissionManager` service.

## See Also

* [Authorization](./index.md)
* [Permission Management Module](../../../modules/permission-management.md)
* [Entities](../../architecture/domain-driven-design/entities.md)
