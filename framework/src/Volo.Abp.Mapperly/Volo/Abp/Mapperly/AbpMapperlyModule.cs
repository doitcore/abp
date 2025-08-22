using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectMapping;

namespace Volo.Abp.Mapperly;

[DependsOn(
    typeof(AbpObjectMappingModule),
    typeof(AbpObjectExtendingModule),
    typeof(AbpAuditingModule)
)]
public class AbpMapperlyModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new AbpMapperlyConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // This is the temporary solution, We will remove it in when all apps are migrated to Mapperly.
        var disableMapperlyAutoObjectMappingProvider = false;

        var modules = context.Services.GetSingletonInstance<IAbpApplication>().Modules.ToList();
        var autoMapperModuleIndex = modules.FindIndex(x => x.Type.FullName!.Equals("Volo.Abp.AutoMapper.AbpAutoMapperModule", StringComparison.OrdinalIgnoreCase));
        if (autoMapperModuleIndex >= 0)
        {
            var mapperlyModuleIndex = modules.FindIndex(x => x.Type == typeof(AbpMapperlyModule));
            if (mapperlyModuleIndex > autoMapperModuleIndex)
            {
                disableMapperlyAutoObjectMappingProvider = true;
            }
        }

        if (!disableMapperlyAutoObjectMappingProvider)
        {
            context.Services.AddMapperlyObjectMapper();
        }
    }
}
