using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.PermissionManagement;

public class TestResourcePermissionDefinitionProvider: PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddResourcePermission("MyResourcePermission1", TestEntityResource.ResourceName);
        context.AddResourcePermission("MyResourceDisabledPermission1", TestEntityResource.ResourceName, isEnabled: false);
        context.AddResourcePermission("MyResourcePermission2", TestEntityResource.ResourceName);
        context.AddResourcePermission("MyResourcePermission3", TestEntityResource.ResourceName, multiTenancySide: MultiTenancySides.Host);
        context.AddResourcePermission("MyResourcePermission4", TestEntityResource.ResourceName, multiTenancySide: MultiTenancySides.Host).WithProviders(UserPermissionValueProvider.ProviderName);

        var myPermission5 = context.AddResourcePermission("MyResourcePermission5", TestEntityResource.ResourceName);
        myPermission5.StateCheckers.Add(new TestRequireRolePermissionStateProvider("super-admin"));

        context.AddResourcePermission("MyResourcePermission6", TestEntityResource.ResourceName);

        context.AddResourcePermission("MyResourceDisabledPermission2", TestEntityResource.ResourceName, isEnabled: false);

        context.AddResourcePermission("MyResourcePermission7", TestEntityResource.ResourceName);
        context.AddResourcePermission("MyResourcePermission8", TestEntityResource.ResourceName);
    }
}
