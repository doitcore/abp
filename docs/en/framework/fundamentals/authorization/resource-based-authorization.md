```json
//[doc-seo]
{
    "Description": "Learn how to implement resource-based authorization in ABP Framework for fine-grained access control on specific resource instances like documents, projects, or any entity."
}
```

# Resource-Based Authorization

Resource-based authorization is a powerful feature that enables fine-grained access control based on specific resource instances. While the standard [permission system](./index.md) grants permissions at a general level (e.g., "can edit documents"), resource-based authorization allows you to grant permissions for a **specific** document, project, or any other entity rather than granting a permission for all of them.

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
                displayName: LocalizableString.Create<BookStoreResource>("Permission:Document:View")
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

### Checking Resource Permissions

Use the `IResourcePermissionChecker` service to check if the current user has a specific permission for a resource:

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

ABP entities implement the `IKeyedObject` interface, which provides a `GetObjectKey()` method. This allows for a cleaner syntax when checking permissions:

```csharp
public async Task<DocumentDto> GetAsync(Guid id)
{
    var document = await _documentRepository.GetAsync(id);

    // Using the entity directly - GetObjectKey() returns the entity's Id
    if (!await _resourcePermissionChecker.IsGrantedAsync(
        "BookStore_Document_View",
        document))
    {
        throw new AbpAuthorizationException(
            "You don't have permission to view this document.");
    }

    return ObjectMapper.Map<Document, DocumentDto>(document);
}
```

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

Once you have defined resource permissions, you need a way to grant or revoke them for specific users, roles, or clients. ABP provides two approaches for managing resource permissions: through built-in **UI components** that offer a user-friendly interface, or **programmatically** using the `IResourcePermissionManager` service for automated scenarios.

#### Resource Permission Management Modal

ABP provides built-in UI components for managing resource permissions on all supported UI frameworks (**MVC/Razor Pages**, **Blazor** and **Angular** UIs) as explained in the following sections.

##### MVC / Razor Pages

You can use the `abp.ModalManager` to open the resource permission management dialog as follows:

```javascript
var _resourcePermissionsModal = new abp.ModalManager(
    abp.appPath + 'AbpPermissionManagement/ResourcePermissionManagementModal'
);

// Open the modal for a specific resource
_resourcePermissionsModal.open({
    resourceName: 'BookStore.Document',
    resourceKey: documentId,
    resourceDisplayName: documentTitle
});
```

##### Blazor

You can use the `ResourcePermissionManagementModal` component in Blazor UI and open the resource management modal by using the `OpenAsync` method:

```xml
<ResourcePermissionManagementModal @ref="ResourcePermissionModal" />

@code {
    private ResourcePermissionManagementModal ResourcePermissionModal { get; set; }

    private async Task OpenPermissionsModal(DocumentDto document)
    {
        await ResourcePermissionModal.OpenAsync(
            resourceName: "BookStore.Document",
            resourceKey: document.Id.ToString(),
            resourceDisplayName: document.Title
        );
    }
}
```

##### Angular

For Angular UI, you can use the `ResourcePermissionManagementComponent` as follows:

```typescript
import { ResourcePermissionManagementComponent } from '@abp/ng.permission-management';

// In your component
openPermissionsModal(document: DocumentDto) {
  const modalRef = this.modalService.open(ResourcePermissionManagementComponent, {
    // modal options
  });
  modalRef.componentInstance.resourceName = 'BookStore.Document';
  modalRef.componentInstance.resourceKey = document.id;
  modalRef.componentInstance.resourceDisplayName = document.title;
}
```

#### Using `IResourcePermissionManager`

For programmatic permission management, you can use the `IResourcePermissionManager` service for getting/setting permissions for resources:

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace Acme.BookStore.Documents
{
    public class DocumentPermissionService : ITransientDependency
    {
        private readonly IResourcePermissionManager _resourcePermissionManager;

        public DocumentPermissionService(
            IResourcePermissionManager resourcePermissionManager)
        {
            _resourcePermissionManager = resourcePermissionManager;
        }

        public async Task GrantViewPermissionToUserAsync(
            Guid documentId, 
            Guid userId)
        {
            await _resourcePermissionManager.SetAsync(
                permissionName: "BookStore_Document_View",
                resourceName: "BookStore.Document",
                resourceKey: documentId.ToString(),
                providerName: "U", // User provider
                providerKey: userId.ToString(),
                isGranted: true
            );
        }

        public async Task GrantEditPermissionToRoleAsync(
            Guid documentId, 
            string roleName)
        {
            await _resourcePermissionManager.SetAsync(
                permissionName: "BookStore_Document_Edit",
                resourceName: "BookStore.Document",
                resourceKey: documentId.ToString(),
                providerName: "R", // Role provider
                providerKey: roleName,
                isGranted: true
            );
        }

        public async Task RevokeAllPermissionsForUserAsync(
            Guid documentId, 
            Guid userId)
        {
            await _resourcePermissionManager.DeleteAsync(
                resourceName: "BookStore.Document",
                resourceKey: documentId.ToString(),
                providerName: "U",
                providerKey: userId.ToString()
            );
        }
    }
}
```

The `IResourcePermissionManager` service allows you to programmatically grant, revoke, and query resource permissions at runtime. This is useful for scenarios like automatically granting permissions when a resource is created, implementing sharing functionality, or integrating with external systems.

> *Note:* When using the built-in UI components (Resource Permission Management Modal), the permission management for users, roles, and clients is automatically handled through the interface without requiring any additional code.

## Integration with Entities

ABP entities automatically implement the `IKeyedObject` interface, which is used by the resource-based authorization system:

```csharp
public interface IKeyedObject
{
    string? GetObjectKey();
}
```

For entities with a primary key, `GetObjectKey()` returns the key as a string. This enables convenient extension methods on `IResourcePermissionChecker`:

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

## Advanced Topics

### Custom Resource Permission Value Providers

Similar to the standard permission system, you can create custom value providers for resource permissions.

ABP comes with two built-in resource permission value providers:

* `UserResourcePermissionValueProvider` (`U`): Checks permissions granted directly to users
* `RoleResourcePermissionValueProvider` (`R`): Checks permissions granted to roles

You can create your own custom value provider by implementing the `IResourcePermissionValueProvider` interface or inheriting from the `ResourcePermissionValueProvider` base class:

```csharp
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions.Resources;

public class OwnerResourcePermissionValueProvider : ResourcePermissionValueProvider
{
    public override string Name => "Owner";

    public OwnerResourcePermissionValueProvider(
        IResourcePermissionStore permissionStore) 
        : base(permissionStore)
    {
    }

    public override async Task<PermissionGrantResult> CheckAsync(
        ResourcePermissionValueCheckContext context)
    {
        // Check if the current user is the owner of the resource
        var currentUserId = context.Principal?.FindUserId();
        if (currentUserId == null)
        {
            return PermissionGrantResult.Undefined;
        }

        // Your logic to determine if user is the owner
        var isOwner = await CheckIfUserIsOwnerAsync(
            currentUserId.Value, 
            context.ResourceName, 
            context.ResourceKey);

        return isOwner 
            ? PermissionGrantResult.Granted 
            : PermissionGrantResult.Undefined;
    }

    private Task<bool> CheckIfUserIsOwnerAsync(
        Guid userId, 
        string resourceName, 
        string resourceKey)
    {
        // Implement your ownership check logic
        throw new NotImplementedException();
    }
}
```

Register your custom provider in your module's `ConfigureServices` method:

```csharp
Configure<AbpPermissionOptions>(options =>
{
    options.ResourceValueProviders.Add<OwnerResourcePermissionValueProvider>();
});
```

### Resource Permission Store

The `IResourcePermissionStore` interface is responsible for retrieving resource permission values. The [Permission Management Module](../../../modules/permission-management.md) provides the default implementation that stores permissions in the database.

You can query the store directly if needed:

```csharp
public class MyService : ITransientDependency
{
    private readonly IResourcePermissionStore _resourcePermissionStore;

    public MyService(IResourcePermissionStore resourcePermissionStore)
    {
        _resourcePermissionStore = resourcePermissionStore;
    }

    public async Task<string[]> GetGrantedResourceKeysAsync(string permissionName)
    {
        // Get all resource keys where the permission is granted
        return await _resourcePermissionStore.GetGrantedResourceKeysAsync(
            "BookStore.Document",
            permissionName);
    }
}
```

### Cleaning Up Resource Permissions

When a resource is deleted, you should clean up its associated permissions to avoid orphaned permission records in the database. You can do this directly in your delete logic or handle it asynchronously through event handlers:

```csharp
public async Task DeleteDocumentAsync(Guid id)
{
    // Delete the document
    await _documentRepository.DeleteAsync(id);

    // Clean up all permissions for this resource
    await _resourcePermissionManager.DeleteAsync(
        resourceName: "BookStore.Document",
        resourceKey: id.ToString(),
        providerName: "U",
        providerKey: null // Deletes for all users
    );

    await _resourcePermissionManager.DeleteAsync(
        resourceName: "BookStore.Document",
        resourceKey: id.ToString(),
        providerName: "R",
        providerKey: null // Deletes for all roles
    );
}
```

> ABP modules automatically handle permission cleanup for their own entities. For your custom entities, you are responsible for cleaning up resource permissions when resources are deleted.

## See Also

* [Authorization](./index.md)
* [Permission Management Module](../../modules/permission-management.md)
* [Entities](../../architecture/domain-driven-design/entities.md)
