using Volo.Abp.AspNetCore;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Security;
using Volo.Abp.VirtualFileSystem;

namespace Volo.Abp.OperationRateLimit;

[DependsOn(
    typeof(AbpCachingModule),
    typeof(AbpLocalizationModule),
    typeof(AbpSecurityModule),
    typeof(AbpAspNetCoreAbstractionsModule),
    typeof(AbpDistributedLockingAbstractionsModule)
)]
public class AbpOperationRateLimitModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpOperationRateLimitModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<AbpOperationRateLimitResource>("en")
                .AddVirtualJson("/Volo/Abp/OperationRateLimit/Localization");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace(
                "Volo.Abp.OperationRateLimit",
                typeof(AbpOperationRateLimitResource));
        });
    }
}
