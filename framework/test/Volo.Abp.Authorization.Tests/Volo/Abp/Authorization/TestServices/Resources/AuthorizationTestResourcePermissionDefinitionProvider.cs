using Shouldly;
using Volo.Abp.Authorization.Permissions;

namespace Volo.Abp.Authorization.TestServices.Resources;

public class AuthorizationTestResourcePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var permission1 = context.AddResourcePermission("MyResourcePermissions1", resourceName: typeof(TestEntityResource).FullName!);
        permission1[PermissionDefinitionContext.KnownPropertyNames.CurrentProviderName].ShouldBe(typeof(AuthorizationTestResourcePermissionDefinitionProvider).FullName);

        context.AddResourcePermission("MyResourcePermissions1", resourceName: typeof(TestEntityResource).FullName!).StateCheckers.Add(new TestRequireEditionPermissionSimpleStateChecker());
        context.AddResourcePermission("MyResourcePermissions2", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermissions3", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermissions4", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermissions5", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermissions6", resourceName: typeof(TestEntityResource).FullName!).WithProviders(nameof(TestPermissionValueProvider1));
        context.AddResourcePermission("MyResourcePermissions7", resourceName: typeof(TestEntityResource).FullName!).WithProviders(nameof(TestPermissionValueProvider2));

        context.GetResourcePermissionOrNull("MyResourcePermissions1").ShouldNotBeNull();
    }
}
