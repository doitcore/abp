using System.Collections.Generic;
using Volo.Abp.Autofac;
using Volo.Abp.Internal.Telemetry.Activity.Providers;
using Volo.Abp.Modularity;

namespace Volo.Abp.Cli;

[DependsOn(
    typeof(AbpCliCoreModule),
    typeof(AbpAutofacModule)
)]
public class AbpCliModule : AbpModule
{

}
