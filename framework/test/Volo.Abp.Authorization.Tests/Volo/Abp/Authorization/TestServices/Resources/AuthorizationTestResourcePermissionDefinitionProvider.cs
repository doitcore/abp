using Shouldly;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace Volo.Abp.Authorization.TestServices.Resources;

public class AuthorizationTestResourcePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var permission1 = context.AddResourcePermission("MyResourcePermission1", resourceName: TestEntityResource.ResourceName);
        Assert.Throws<AbpException>(() =>
        {
            permission1.AddChild("MyResourcePermission1.ChildPermission1");
        }).Message.ShouldBe($"Resource permission cannot have child permissions. Resource: {TestEntityResource.ResourceName}");
        permission1.StateCheckers.Add(new TestRequireEditionPermissionSimpleStateChecker());;
        permission1[PermissionDefinitionContext.KnownPropertyNames.CurrentProviderName].ShouldBe(typeof(AuthorizationTestResourcePermissionDefinitionProvider).FullName);

        context.AddResourcePermission("MyResourcePermission2", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermission3", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermission4", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermission5", resourceName: typeof(TestEntityResource).FullName!);
        context.AddResourcePermission("MyResourcePermission6", resourceName: typeof(TestEntityResource).FullName!).WithProviders(nameof(TestResourcePermissionValueProvider1));
        context.AddResourcePermission("MyResourcePermission7", resourceName: typeof(TestEntityResource).FullName!).WithProviders(nameof(TestResourcePermissionValueProvider2));

        context.GetResourcePermissionOrNull("MyResourcePermission1").ShouldNotBeNull();
    }
}
